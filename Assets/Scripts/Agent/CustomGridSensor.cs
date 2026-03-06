using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// Custom ISensor that provides a 11x11 ego-centric view centered on the agent.
/// 
/// Channel layout (5 channels):
///   [0] Enemy exists (1.0)
///   [1] Enemy is stronger than agent (1.0)
///   [2] Enemy is weaker than agent (1.0)
///   [3] Pickup present (1.0)
///   [4] Wall or out-of-bounds (1.0)
/// </summary>
public class CustomGridSensor : ISensor
{
    // ---- configuration ----
    private int viewRadius;
    private int viewSize;
    private const int Channels = 5;

    // ---- references set externally ----
    private Grid grid;
    private GridPlaceable agentPlaceable;
    private DamageResolver agentDamageResolver;

    // ---- cached spec ----
    private ObservationSpec observationSpec;

    public CustomGridSensor(Grid grid, int viewRadius)
    {
        this.grid = grid;
        this.viewRadius = viewRadius;
        this.viewSize = viewRadius * 2 + 1;
        observationSpec = ObservationSpec.Visual(Channels, viewSize, viewSize);
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
                for (int h = 0; h < viewSize; h++)
                    for (int w = 0; w < viewSize; w++)
                        writer[c, h, w] = 0f;
            return viewSize * viewSize * Channels;
        }

        Vector2Int center = agentPlaceable.Position;
        int agentPow = agentDamageResolver.power;

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
                    writer[0, row, col] = 0f; // enemy exists
                    writer[1, row, col] = 0f; // enemy is stronger
                    writer[2, row, col] = 0f; // enemy is weaker
                    writer[3, row, col] = 0f; // pickup
                    writer[4, row, col] = 1f; // out-of-bounds treated as wall
                    continue;
                }

                float enemyExists = 0f;
                float enemyIsStronger = 0f;
                float enemyIsWeaker = 0f;
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
                                enemyExists = 1f;
                                int enemyPow = gp.Entity.damageResolver.power;
                                if (enemyPow > agentPow)
                                    enemyIsStronger = 1f;
                                else if (enemyPow < agentPow)
                                    enemyIsWeaker = 1f;
                            }
                            break;

                        case GridPlaceable.PlaceableType.Pickup:
                            pickupChannel = 1f;
                            break;
                    }
                }

                writer[0, row, col] = enemyExists;
                writer[1, row, col] = enemyIsStronger;
                writer[2, row, col] = enemyIsWeaker;
                writer[3, row, col] = pickupChannel;
                writer[4, row, col] = wallChannel;
            }
        }

        return viewSize * viewSize * Channels;
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

        for (int dy = -viewRadius; dy <= viewRadius; dy++)
        {
            for (int dx = -viewRadius; dx <= viewRadius; dx++)
            {
                Vector2Int worldCell = center + new Vector2Int(dx, dy);
                Vector3 worldPos = grid.GetWorldPosition(worldCell);

                float enemyExists = 0f;
                float enemyIsStronger = 0f;
                float enemyIsWeaker = 0f;
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
                                    enemyExists = 1f;
                                    int enemyPow = gp.Entity.damageResolver.power;
                                    if (enemyPow > agentPow)
                                        enemyIsStronger = 1f;
                                    else if (enemyPow < agentPow)
                                        enemyIsWeaker = 1f;
                                }
                                break;
                        }
                    }
                }

                // Draw cell: Red=Threat, Yellow=Prey, Orange=Equal, Green=Pickup, Blue=Wall
                float r = 0, g = 0, b = 0;

                if (enemyExists > 0)
                {
                    if (enemyIsStronger > 0) { r = 1f; g = 0f; } // Red
                    else if (enemyIsWeaker > 0) { r = 1f; g = 1f; } // Yellow
                    else { r = 1f; g = 0.5f; } // Orange (Equal power)
                }

                if (pickupChannel > 0) { g = 1f; } // Green (Additive if overlapping)
                if (wallChannel > 0) { b = 1f; }   // Blue (Additive)

                Color color = new Color(r, g, b, 0.3f);
                if (enemyExists == 0 && pickupChannel == 0 && wallChannel == 0)
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
