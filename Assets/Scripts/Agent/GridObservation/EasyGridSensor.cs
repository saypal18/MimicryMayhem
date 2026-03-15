using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// Custom ISensor that provides an ego-centric view centered on the agent.
///
/// Channel layout (4 channels):
///   [0] Enemy exists (1.0)
///   [1] Pickup present (1.0)
///   [2] Bush present (1.0)
///   [3] Wall or out-of-bounds (1.0)
/// </summary>
public class EasyGridSensor : ISensor
{
    private int viewRadius;
    private int viewSize;
    private const int Channels = 4;

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

        for (int dy = -viewRadius; dy <= viewRadius; dy++)
        {
            for (int dx = -viewRadius; dx <= viewRadius; dx++)
            {
                int row = dy + viewRadius;
                int col = dx + viewRadius;

                Vector2Int worldCell = center + new Vector2Int(dx, dy);

                // Out-of-bounds?
                var tile = grid.GetTile(worldCell);
                if (tile == null)
                {
                    writer[0, row, col] = 0f; // enemy
                    writer[1, row, col] = 0f; // pickup
                    writer[2, row, col] = 0f; // bush
                    writer[3, row, col] = 1f; // out-of-bounds treated as wall
                    continue;
                }

                float enemyChannel = 0f;
                float pickupChannel = 0f;
                float bushChannel = 0f;
                float wallChannel = 0f;

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

                            case GridPlaceable.PlaceableType.Entity:
                                if (gp != agentPlaceable)
                                {
                                    enemyChannel = 1f;
                                }
                                break;

                            case GridPlaceable.PlaceableType.Pickup:
                                pickupChannel = 1f;
                                break;
                        }
                    }
                }

                writer[0, row, col] = enemyChannel;
                writer[1, row, col] = pickupChannel;
                writer[2, row, col] = bushChannel;
                writer[3, row, col] = wallChannel;
            }
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

                float enemyChannel = 0f;
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
                                    if (gp != agentPlaceable)
                                    {
                                        enemyChannel = 1f;
                                    }
                                    break;
                            }
                        }
                    }
                }

                // Draw cell: Red=Threat, Green=Pickup, Blue=Wall, Cyan=Bush
                float r = 0, g = 0, b = 0;

                if (enemyChannel > 0) { r = 1f; g = 0f; } // Red
                if (pickupChannel > 0) { g = 1f; } // Green (Additive if overlapping)
                if (wallChannel > 0) { b = 1f; }   // Blue (Additive)
                if (bushChannel > 0) { r = 0f; g = 1f; b = 1f; } // Cyan (Bush overrides)

                Color color = new Color(r, g, b, 0.3f);
                if (enemyChannel == 0 && pickupChannel == 0 && wallChannel == 0 && bushChannel == 0)
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
