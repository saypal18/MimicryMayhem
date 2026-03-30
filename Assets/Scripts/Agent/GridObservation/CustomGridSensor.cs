using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// Custom ISensor that provides a 11x11 ego-centric view centered on the agent.
///
/// Channel layout (6 channels):
///   [0] Enemy exists (1.0)
///   [1] Enemy is stronger than agent (1.0)
///   [2] Enemy is weaker than agent (1.0)
///   [3] Pickup present (1.0)
///   [4] Wall or out-of-bounds (1.0)
///   [5] Bush present (1.0)
/// </summary>
public class CustomGridSensor : ISensor
{
    // ---- configuration ----
    private int viewRadius;
    private int viewSize;
    private const int Channels = 6;

    public bool LogObservations = false;

    // ---- references set externally ----
    private Grid grid;
    private GridPlaceable agentPlaceable;
    private DamageDealer agentDamageResolver;

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
    public void SetAgentReferences(GridPlaceable placeable, DamageDealer damageResolver, Grid grid)
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
        int agentPow = agentDamageResolver.tier;
        float[,,] observations = new float[Channels, viewSize, viewSize];

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
                    observations[4, row, col] = 1f; // out-of-bounds treated as wall
                    continue;
                }

                float enemyExists = 0f;
                float enemyIsStronger = 0f;
                float enemyIsWeaker = 0f;
                float pickupChannel = 0f;
                float wallChannel = 0f;
                float bushChannel = 0f;

                bool hasBush = false;
                foreach (GridPlaceable gp in tile)
                {
                    if (gp.Type == GridPlaceable.PlaceableType.Bush)
                    {
                        hasBush = true;
                        break;
                    }
                }

                foreach (GridPlaceable gp in tile)
                {
                    switch (gp.Type)
                    {
                        case GridPlaceable.PlaceableType.Bush:
                            bushChannel = 1f;
                            break;

                        case GridPlaceable.PlaceableType.Wall:
                            wallChannel = 1f;
                            break;

                        case GridPlaceable.PlaceableType.Entity:
                            if (hasBush) break;
                            if (gp != agentPlaceable && gp.Entity != null)
                            {
                                enemyExists = 1f;
                                if (gp.Entity.damageDealer != null)
                                {
                                    int enemyPow = gp.Entity.damageDealer.tier;
                                    if (enemyPow > agentPow)
                                        enemyIsStronger = 1f;
                                    else if (enemyPow < agentPow)
                                        enemyIsWeaker = 1f;
                                }
                            }
                            break;

                        case GridPlaceable.PlaceableType.Pickup:
                            if (hasBush) break;
                            pickupChannel = 1f;
                            break;
                    }
                }

                observations[0, row, col] = enemyExists;
                observations[1, row, col] = enemyIsStronger;
                observations[2, row, col] = enemyIsWeaker;
                observations[3, row, col] = pickupChannel;
                observations[4, row, col] = wallChannel;
                observations[5, row, col] = bushChannel;
            }
        }

        // Write to ML-Agents writer
        for (int c = 0; c < Channels; c++)
        {
            for (int r = 0; r < viewSize; r++)
            {
                for (int w = 0; w < viewSize; w++)
                {
                    writer[c, r, w] = observations[c, r, w];
                }
            }
        }

        if (LogObservations && agentPlaceable != null && agentPlaceable.Entity != null)
        {
            string[] channelNames = { "Enemy Exists", "Enemy Stronger", "Enemy Weaker", "Pickup", "Wall", "Bush" };
            string log = $"CustomGridSensor - {agentPlaceable.Entity.name} (Team {agentPlaceable.Entity.TeamId}) - {viewSize}x{viewSize}\n";
            for (int c = 0; c < Channels; c++)
            {
                log += $"Channel {c} ({channelNames[c]}):\n";
                for (int r = viewSize - 1; r >= 0; r--)
                {
                    for (int w = 0; w < viewSize; w++)
                    {
                        log += observations[c, r, w] > 0.5f ? "# " : ". ";
                    }
                    log += "\n";
                }
            }
            Debug.Log(log);
        }

        return viewSize * viewSize * Channels;
    }

    public void Reset() { }
    public void Update() { }
    public byte[] GetCompressedObservation() => null;
}
