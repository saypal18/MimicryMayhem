using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// Custom ISensor that provides an ego-centric view centered on the agent.
///
/// Channel layout (5 channels):
///   [0] Self team (1.0)
///   [1] Enemy team (1.0)
///   [2] Pickup present (1.0)
///   [3] Bush present (1.0)
///   [4] Wall or out-of-bounds (1.0)
/// </summary>
public class EasyGridSensor : ISensor
{
    private int viewRadius;
    private int viewSize;
    private const int Channels = 5;

    public bool LogObservations = false;

    private Grid grid;
    private GridPlaceable agentPlaceable;

    private ObservationSpec observationSpec;

    public EasyGridSensor(Grid grid, int viewRadius)
    {
        this.grid = grid;
        this.viewRadius = viewRadius;
        this.viewSize = viewRadius * 2 + 1;
        observationSpec = ObservationSpec.Visual(Channels, viewSize, viewSize);
    }

    public void SetAgentReferences(GridPlaceable placeable, Grid grid)
    {
        this.grid = grid;
        agentPlaceable = placeable;
    }

    // ---- ISensor ----

    public string GetName() => "EasyGridSensor";

    public ObservationSpec GetObservationSpec() => observationSpec;

    public CompressionSpec GetCompressionSpec() => CompressionSpec.Default();

    public int Write(ObservationWriter writer)
    {
        if (agentPlaceable == null)
        {
            // Fill with zeros if not initialised yet
            for (int c = 0; c < Channels; c++)
                for (int h = 0; h < viewSize; h++)
                    for (int w = 0; w < viewSize; w++)
                        writer[c, h, w] = 0f;
            return viewSize * viewSize * Channels;
        }

        Vector2Int center = agentPlaceable.Position;

        float[,,] logBuffer = LogObservations ? new float[Channels, viewSize, viewSize] : null;

        for (int dy = -viewRadius; dy <= viewRadius; dy++)
        {
            for (int dx = -viewRadius; dx <= viewRadius; dx++)
            {
                int row = dy + viewRadius;
                int col = dx + viewRadius;

                Vector2Int worldCell = center + new Vector2Int(dx, dy);

                float[] channelValues = new float[Channels];

                // Out-of-bounds?
                var tile = grid.GetTile(worldCell);
                if (tile == null)
                {
                    channelValues[4] = 1f; // wall
                }
                else
                {
                    bool hasBush = false;
                    foreach (GridPlaceable gp in tile)
                    {
                        if (gp.Type == GridPlaceable.PlaceableType.Bush)
                        {
                            hasBush = true;
                            channelValues[3] = 1f; // bush
                            break;
                        }
                    }

                    if (!hasBush)
                    {
                        foreach (GridPlaceable gp in tile)
                        {
                            switch (gp.Type)
                            {
                                case GridPlaceable.PlaceableType.Wall:
                                    channelValues[4] = 1f;
                                    break;

                                case GridPlaceable.PlaceableType.Entity:
                                    if (gp.Entity != null && agentPlaceable != null && agentPlaceable.Entity != null)
                                    {
                                        bool gpHasTeam = gp.Entity.behaviorParameters != null;
                                        bool agentHasTeam = agentPlaceable.Entity.behaviorParameters != null;

                                        if (gpHasTeam && agentHasTeam)
                                        {
                                            if (gp.Entity.TeamId == agentPlaceable.Entity.TeamId)
                                                channelValues[0] = 1f;
                                            else
                                                channelValues[1] = 1f;
                                        }
                                        else
                                        {
                                            if (gp == agentPlaceable) channelValues[0] = 1f;
                                            else channelValues[1] = 1f;
                                        }
                                    }
                                    break;

                                case GridPlaceable.PlaceableType.Pickup:
                                    channelValues[2] = 1f;
                                    break;
                            }
                        }
                    }
                }

                for (int c = 0; c < Channels; c++)
                {
                    writer[c, row, col] = channelValues[c];
                    if (LogObservations) logBuffer[c, row, col] = channelValues[c];
                }
            }
        }

        if (LogObservations)
        {
            string[] channelNames = { "Self Team", "Enemy Team", "Pickup", "Bush", "Wall" };
            string log = $"EasyGridSensor - {agentPlaceable.Entity.name} (Team {agentPlaceable.Entity.TeamId}) - {viewSize}x{viewSize}\n";
            for (int c = 0; c < Channels; c++)
            {
                log += $"Channel {c} ({channelNames[c]}):\n";
                for (int r = viewSize - 1; r >= 0; r--)
                {
                    for (int w = 0; w < viewSize; w++)
                    {
                        // Using # and . for better visual distinction in console
                        log += logBuffer[c, r, w] > 0.5f ? "# " : "O ";
                    }
                    log += "\n";
                }
            }
            Debug.Log(log);
        }

        return viewSize * viewSize * Channels;
    }

    public void OnDrawGizmos()
    {
        if (agentPlaceable == null || grid == null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(agentPlaceable != null ? agentPlaceable.transform.position + Vector3.left : Vector3.left,
                           agentPlaceable != null ? agentPlaceable.transform.position + Vector3.right : Vector3.right);
            return;
        }

        Vector2Int center = agentPlaceable.Position;

        for (int dy = -viewRadius; dy <= viewRadius; dy++)
        {
            for (int dx = -viewRadius; dx <= viewRadius; dx++)
            {
                Vector2Int worldCell = center + new Vector2Int(dx, dy);
                Vector3 worldPos = grid.GetWorldPosition(worldCell);

                float selfTeamChannel = 0f;
                float enemyTeamChannel = 0f;
                float pickupChannel = 0f;
                float bushChannel = 0f;
                float wallChannel = 0f;

                var tile = grid.GetTile(worldCell);
                if (tile == null)
                {
                    wallChannel = 1f;
                }
                else
                {
                    bool hasBush = false;
                    foreach (GridPlaceable gp in tile)
                    {
                        if (gp.Type == GridPlaceable.PlaceableType.Bush)
                        {
                            hasBush = true;
                            bushChannel = 1f;
                            break;
                        }
                    }

                    if (!hasBush)
                    {
                        foreach (GridPlaceable gp in tile)
                        {
                            switch (gp.Type)
                            {
                                case GridPlaceable.PlaceableType.Wall:
                                    wallChannel = 1f;
                                    break;
                                case GridPlaceable.PlaceableType.Pickup:
                                    pickupChannel = 1f;
                                    break;
                                case GridPlaceable.PlaceableType.Entity:
                                    if (gp.Entity != null && agentPlaceable != null && agentPlaceable.Entity != null)
                                    {
                                        bool gpHasTeam = gp.Entity.behaviorParameters != null;
                                        bool agentHasTeam = agentPlaceable.Entity.behaviorParameters != null;

                                        if (gpHasTeam && agentHasTeam)
                                        {
                                            if (gp.Entity.TeamId == agentPlaceable.Entity.TeamId)
                                            {
                                                selfTeamChannel = 1f;
                                            }
                                            else
                                            {
                                                enemyTeamChannel = 1f;
                                            }
                                        }
                                        else
                                        {
                                            if (gp == agentPlaceable) selfTeamChannel = 1f;
                                            else enemyTeamChannel = 1f;
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }

                // Draw cell: Blue=SelfTeam, Red=EnemyTeam, Green=Pickup, Cyan=Bush, White=Wall
                float r = 0, g = 0, b = 0;

                if (selfTeamChannel > 0) { b = 1f; } // Blue
                if (enemyTeamChannel > 0) { r = 1f; } // Red
                if (pickupChannel > 0) { g = 1f; } // Green
                if (wallChannel > 0) { r = 1f; g = 1f; b = 1f; }   // White
                if (bushChannel > 0) { g = 1f; b = 1f; r = 0f; } // Cyan

                Color color = new Color(r, g, b, 0.3f);
                if (selfTeamChannel == 0 && enemyTeamChannel == 0 && pickupChannel == 0 && wallChannel == 0 && bushChannel == 0)
                    color.a = 0.05f;

                Gizmos.color = color;
                Vector3 cubeSize = new Vector3(grid.TileSize.x * 0.9f, grid.TileSize.y * 0.9f, 0.1f);
                Gizmos.DrawCube(worldPos, cubeSize);

                // Outline for the whole view
                if (dx == -viewRadius && dy == -viewRadius)
                {
                    Gizmos.color = Color.white;
                    Vector3 viewCenter = grid.GetWorldPosition(center);
                    Vector3 totalViewSize = new Vector3(viewSize * grid.TileSize.x, viewSize * grid.TileSize.y, 0.1f);
                    Gizmos.DrawWireCube(viewCenter, totalViewSize);
                }
            }
        }
    }

    public byte[] GetCompressedObservation() => null;

    public void Update() { }

    public void Reset() { }
}
