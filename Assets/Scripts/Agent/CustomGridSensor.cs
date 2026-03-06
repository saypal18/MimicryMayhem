using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// Custom ISensor that provides a 11x11 ego-centric view centered on the agent.
/// 
/// Channel layout (3 channels):
///   [0] Enemy power ratio = Clamp01((enemyPower/agentPower - minRatio) / (maxRatio - minRatio))
///       0.0  -> no enemy present
///       below middle -> weaker enemy (prey)
///       above middle -> stronger enemy (threat)
///   [1] Pickup present  (0 or 1)
///   [2] Wall or out-of-bounds (0 or 1)
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

    // ---- tunable thresholds ----
    private float minRatio;
    private float maxRatio;

    // ---- cached spec ----
    private ObservationSpec observationSpec;

    public CustomGridSensor(Grid grid, float minRatio = 0.1f, float maxRatio = 10f)
    {
        this.grid = grid;
        this.minRatio = minRatio;
        this.maxRatio = maxRatio;
        observationSpec = ObservationSpec.Visual(ViewSize, ViewSize, Channels);
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
            for (int h = 0; h < ViewSize; h++)
                for (int w = 0; w < ViewSize; w++)
                    for (int c = 0; c < Channels; c++)
                        writer[h, w, c] = 0f;
            return ViewSize * ViewSize * Channels;
        }

        Vector2Int center = agentPlaceable.Position;
        int agentPow = Mathf.Max(1, agentDamageResolver.power); // guard div-by-zero

        for (int dy = -ViewRadius; dy <= ViewRadius; dy++)
        {
            for (int dx = -ViewRadius; dx <= ViewRadius; dx++)
            {
                // Map to ObservationWriter indices.
                // MLAgents Visual: writer[height_row, width_col, channel]
                // We treat dy as the row axis (y) and dx as the column axis (x).
                int row = dy + ViewRadius;
                int col = dx + ViewRadius;

                Vector2Int worldCell = center + new Vector2Int(dx, dy);

                // Out-of-bounds?
                var tile = grid.GetTile(worldCell);
                if (tile == null)
                {
                    writer[row, col, 0] = 0f; // no enemy
                    writer[row, col, 1] = 0f; // no pickup
                    writer[row, col, 2] = 1f; // out-of-bounds treated as wall
                    continue;
                }

                float enemyChannel = 0f;
                float pickupChannel = 0f;
                float wallChannel = 0f;

                foreach (GridPlaceable gp in tile)
                {
                    // Wall?
                    if (gp.CompareTag("Wall"))
                    {
                        wallChannel = 1f;
                        continue;
                    }

                    // Entity (enemy)?
                    if (gp.TryGetComponent(out Entity entity))
                    {
                        if (gp != agentPlaceable)          // skip self
                        {
                            int enemyPow = entity.damageResolver.power;
                            float ratio = enemyPow / (float)agentPow;
                            float norm = Mathf.Clamp01((ratio - minRatio) / (maxRatio - minRatio));
                            enemyChannel = Mathf.Max(enemyChannel, norm); // take strongest if multiple
                        }
                        continue;
                    }

                    // Pickup? (Pickup is an interface; check the concrete MonoBehaviour type)
                    if (gp.TryGetComponent(out GrowPickup _))
                    {
                        pickupChannel = 1f;
                    }
                }

                writer[row, col, 0] = enemyChannel;
                writer[row, col, 1] = pickupChannel;
                writer[row, col, 2] = wallChannel;
            }
        }

        return ViewSize * ViewSize * Channels;
    }

    public byte[] GetCompressedObservation() => null;

    public void Update() { }

    public void Reset() { }
}
