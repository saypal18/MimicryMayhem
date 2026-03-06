using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// Custom ISensor that provides a 11x11 ego-centric view centered on the agent.
/// 
/// Channel layout (3 channels):
///   [0] Enemy power representation:
///       0.0      -> no enemy
///       0.2..0.4 -> weaker enemy (prey)
///       0.5..0.7 -> roughly equal
///       0.8..1.0 -> stronger enemy (threat)
///   [1] Pickup present (1.0)
///   [2] Wall or out-of-bounds (1.0)
/// </summary>
public class CustomGridSensor : ISensor
{
    // ---- constants ----
    private const int ViewRadius = 5;         // half-size: gives an 11x11 window
    private const int ViewSize = ViewRadius * 2 + 1; // 11
    private const int Channels = 3;

    // ---- references set externally ----
    private Grid grid;
    private GridPlaceable agentPlaceable;
    private DamageResolver agentDamageResolver;

    // ---- cached spec ----
    private ObservationSpec observationSpec;

    public CustomGridSensor(Grid grid)
    {
        this.grid = grid;
        observationSpec = ObservationSpec.Visual(Channels, ViewSize, ViewSize);
    }

    // Called by SurvivorAgent.Initialize() so each agent instance has its own POV
    public void SetAgentReferences(GridPlaceable placeable, DamageResolver damageResolver, Grid grid)
    {
        this.grid = grid;
        agentPlaceable = placeable;
        agentDamageResolver = damageResolver;
    }

    // ---- ISensor ----

    public string GetName() => "CustomGridSensor";

    public ObservationSpec GetObservationSpec() => observationSpec;

    public CompressionSpec GetCompressionSpec() => CompressionSpec.Default();

    public int Write(ObservationWriter writer)
    {
        if (agentPlaceable == null || agentDamageResolver == null)
        {
            // Fill with zeros if not initialised yet
            for (int c = 0; c < Channels; c++)
                for (int h = 0; h < ViewSize; h++)
                    for (int w = 0; w < ViewSize; w++)
                        writer[c, h, w] = 0f;
            return ViewSize * ViewSize * Channels;
        }

        Vector2Int center = agentPlaceable.Position;
        int agentPow = Mathf.Max(1, agentDamageResolver.power); // guard div-by-zero

        for (int dy = -ViewRadius; dy <= ViewRadius; dy++)
        {
            for (int dx = -ViewRadius; dx <= ViewRadius; dx++)
            {
                // Map to ObservationWriter indices.
                // MLAgents Visual: writer[channel, height_row, width_col]
                // We treat dy as the row axis (y) and dx as the column axis (x).
                int row = dy + ViewRadius;
                int col = dx + ViewRadius;

                Vector2Int worldCell = center + new Vector2Int(dx, dy);

                // Out-of-bounds?
                var tile = grid.GetTile(worldCell);
                if (tile == null)
                {
                    writer[0, row, col] = 0f; // no enemy
                    writer[1, row, col] = 0f; // no pickup
                    writer[2, row, col] = 1f; // out-of-bounds treated as wall
                    continue;
                }

                float enemyChannel = 0f;
                float pickupChannel = 0f;
                float wallChannel = 0f;

                foreach (GridPlaceable gp in tile)
                {
                    switch (gp.Type)
                    {
                        case GridPlaceable.PlaceableType.Wall:
                            wallChannel = 1f;
                            break;

                        case GridPlaceable.PlaceableType.Entity:
                            if (gp != agentPlaceable && gp.Entity != null)
                            {
                                int enemyPow = gp.Entity.damageResolver.power;
                                int diff = enemyPow - agentPow;

                                float powerVal;
                                if (diff < 0)
                                {
                                    // Weaker: Map diff (-5 to -1) -> (0.2 to 0.4)
                                    powerVal = 0.4f + (Mathf.Max(diff, -5) + 1) * 0.05f;
                                }
                                else if (diff == 0)
                                {
                                    powerVal = 0.5f; // Equal
                                }
                                else
                                {
                                    // Stronger: Map diff (1 to 5) -> (0.8 to 1.0)
                                    powerVal = 0.8f + (Mathf.Min(diff, 5) - 1) * 0.05f;
                                }

                                enemyChannel = Mathf.Max(enemyChannel, powerVal);
                            }
                            break;

                        case GridPlaceable.PlaceableType.Pickup:
                            pickupChannel = 1f;
                            break;
                    }
                }

                writer[0, row, col] = enemyChannel;
                writer[1, row, col] = pickupChannel;
                writer[2, row, col] = wallChannel;
            }
        }

        return ViewSize * ViewSize * Channels;
    }

    public void OnDrawGizmos()
    {
        if (agentPlaceable == null || grid == null || agentDamageResolver == null)
        {
            // Draw a red "X" if references are missing
            Gizmos.color = Color.red;
            Gizmos.DrawLine(agentPlaceable != null ? agentPlaceable.transform.position + Vector3.left : Vector3.left,
                           agentPlaceable != null ? agentPlaceable.transform.position + Vector3.right : Vector3.right);
            return;
        }

        Vector2Int center = agentPlaceable.Position;
        int agentPow = Mathf.Max(1, agentDamageResolver.power);

        for (int dy = -ViewRadius; dy <= ViewRadius; dy++)
        {
            for (int dx = -ViewRadius; dx <= ViewRadius; dx++)
            {
                Vector2Int worldCell = center + new Vector2Int(dx, dy);
                Vector3 worldPos = grid.GetWorldPosition(worldCell);

                float enemyChannel = 0f;
                float pickupChannel = 0f;
                float wallChannel = 0f;

                var tile = grid.GetTile(worldCell);
                if (tile == null)
                {
                    wallChannel = 1f;
                }
                else
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
                                if (gp != agentPlaceable && gp.Entity != null)
                                {
                                    int enemyPow = gp.Entity.damageResolver.power;
                                    int diff = enemyPow - agentPow;
                                    float powerVal;
                                    if (diff < 0)
                                        powerVal = 0.4f + (Mathf.Max(diff, -5) + 1) * 0.05f;
                                    else if (diff == 0)
                                        powerVal = 0.5f;
                                    else
                                        powerVal = 0.8f + (Mathf.Min(diff, 5) - 1) * 0.05f;

                                    enemyChannel = Mathf.Max(enemyChannel, powerVal);
                                }
                                break;
                        }
                    }
                }

                // Draw cell
                Color color = new Color(enemyChannel, pickupChannel, wallChannel, 0.3f);
                if (enemyChannel == 0 && pickupChannel == 0 && wallChannel == 0)
                    color.a = 0.05f; // very faint for empty spaces

                Gizmos.color = color;
                Vector3 cubeSize = new Vector3(grid.TileSize.x * 0.9f, grid.TileSize.y * 0.9f, 0.1f);
                Gizmos.DrawCube(worldPos, cubeSize);

                // Outline for the whole view
                if (dx == -ViewRadius && dy == -ViewRadius)
                {
                    Gizmos.color = Color.white;
                    Vector3 viewCenter = grid.GetWorldPosition(center);
                    Vector3 totalViewSize = new Vector3(ViewSize * grid.TileSize.x, ViewSize * grid.TileSize.y, 0.1f);
                    Gizmos.DrawWireCube(viewCenter, totalViewSize);
                }
            }
        }
    }

    public byte[] GetCompressedObservation() => null;

    public void Update() { }

    public void Reset() { }
}
