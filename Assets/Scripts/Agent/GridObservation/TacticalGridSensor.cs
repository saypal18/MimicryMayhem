using Unity.MLAgents.Sensors;
using UnityEngine;
using System.Collections.Generic;
using Unity.MLAgents.Policies;

/// <summary>
/// Advanced ISensor that provides an ego-centric view with tactical information.
///
/// Channel layout (6 channels):
///   [0] Allied team (visual position)
///   [1] Enemy team (visual position)
///   [2] Pickup present (1.0 if inventory has space)
///   [3] Bush present (1.0)
///   [4] Wall or out-of-bounds (1.0)
///   [5] Next attack reach (1.0 where Manhattan dist <= reach)
/// </summary>
public class TacticalGridSensor : ISensor
{
    private int viewRadius;
    private int viewSize;
    private const int Channels = 6;

    public bool LogObservations = false;

    // private Grid grid;
    // private GridPlaceable agentPlaceable;
    private Entity agentEntity;
    // private EntitySpawner entitySpawner;

    private ObservationSpec observationSpec;

    public TacticalGridSensor(int viewRadius, Entity agentEntity)
    {
        // this.grid = grid;
        this.viewRadius = viewRadius;
        this.viewSize = viewRadius * 2 + 1;
        // this.entitySpawner = spawner;
        this.agentEntity = agentEntity;
        // this.agentPlaceable = placeable;
        observationSpec = ObservationSpec.Visual(Channels, viewSize, viewSize);
    }

    public void SetAgentReferences( Entity agentEntity)
    {
        //    Debug.Log("setting agent");
        //    Debug.Log(placeable);
        //    Debug.Log(grid);

        //    this.grid = grid;
        //    this.agentPlaceable = placeable;
        //    this.entitySpawner = spawner;
        this.agentEntity = agentEntity;
    }

    // ---- ISensor ----

    public string GetName() => "TacticalGridSensor";

    public ObservationSpec GetObservationSpec() => observationSpec;

    public CompressionSpec GetCompressionSpec() => CompressionSpec.Default();

    public int Write(ObservationWriter writer)
    {
        // if (agentEntity != null && agentEntity.TryGetComponent(out BehaviorParameters bp) && bp.BehaviorType == BehaviorType.InferenceOnly)
        // {
        //     Debug.Log("Gotcha");
        //     Debug.Log(agentPlaceable);
        //     Debug.Log(grid);
        //     Debug.Log(entitySpawner);
        //     Debug.Log(agentEntity);
        // }
        //if (LogObservations)
        //{
        //    Debug.Log("here");
        //}
        //if (agentEntity.GetComponent<CustomGridSensorComponent>().logObservations)
        //{
        //    Debug.Log("here");
        //}
        GridPlaceable agentPlaceable = agentEntity.gridPlaceable;
        Grid grid = agentEntity.CurrentGrid;
        EntitySpawner entitySpawner = agentEntity.entitySpawner;
        if (agentPlaceable == null || grid == null || entitySpawner == null || agentEntity == null)
        {
            //Debug.Log("NO");
            // Fill with zeros if not initialized
            for (int c = 0; c < Channels; c++)
                for (int h = 0; h < viewSize; h++)
                    for (int w = 0; w < viewSize; w++)
                        writer[c, h, w] = 0f;
            return viewSize * viewSize * Channels;
        }
        //Debug.Log("at least");
        Vector2Int center = agentPlaceable.Position;
        float[,,] observations = new float[Channels, viewSize, viewSize];

        // 1. Static and Inventory-conditional features
        bool canSeePickups = false;
        if (agentEntity != null && agentEntity.inventory != null)
        {
            var slots = agentEntity.inventory.GetSlots();
            foreach (var slot in slots)
            {
                if (slot.item == null)
                {
                    canSeePickups = true;
                    break;
                }
            }
        }

        for (int dy = -viewRadius; dy <= viewRadius; dy++)
        {
            for (int dx = -viewRadius; dx <= viewRadius; dx++)
            {
                int row = dy + viewRadius;
                int col = dx + viewRadius;
                Vector2Int worldCell = center + new Vector2Int(dx, dy);

                var tile = grid.GetTile(worldCell);
                if (tile == null)
                {
                    observations[4, row, col] = 1f; // Out-of-bounds = Wall
                }
                else
                {
                    foreach (GridPlaceable gp in tile)
                    {
                        if (gp.Type == GridPlaceable.PlaceableType.Wall)
                        {
                            observations[4, row, col] = 1f;
                        }
                        else if (gp.Type == GridPlaceable.PlaceableType.Bush)
                        {
                            observations[3, row, col] = 1f;
                        }
                        else if (canSeePickups && gp.Type == GridPlaceable.PlaceableType.Pickup)
                        {
                            observations[2, row, col] = 1f;
                        }
                    }
                }
            }
        }

        // 2. Dynamic features: Entities (Visual Position)
        var activeEntities = entitySpawner.GetActiveEntities();
        foreach (var entity in activeEntities)
        {
            if (entity == null) continue;

            // Convert transform position to grid cell
            Vector2Int visualGridPos = grid.GetGridPosition(entity.transform.position);
            int dx = visualGridPos.x - center.x;
            int dy = visualGridPos.y - center.y;

            if (Mathf.Abs(dx) <= viewRadius && Mathf.Abs(dy) <= viewRadius)
            {
                int row = dy + viewRadius;
                int col = dx + viewRadius;

                if (entity.TeamId == agentEntity.TeamId)
                    observations[0, row, col] = 1f;
                else
                    observations[1, row, col] = 1f;
            }
        }

        // 3. Attack range
        int range = 0;
        if (agentEntity.equippedItem != null)
        {
            var equipped = agentEntity.equippedItem.Get();
            if (equipped is WeaponItem weapon)
            {
                range = weapon.range;
            }
        }

        if (range > 0)
        {
            for (int dy = -viewRadius; dy <= viewRadius; dy++)
            {
                for (int dx = -viewRadius; dx <= viewRadius; dx++)
                {
                    // Cardinal direction constraint: only tiles in a straight line from the agent
                    bool isCardinal = (dx == 0 || dy == 0);
                    int dist = Mathf.Abs(dx) + Mathf.Abs(dy);
                    if (isCardinal && dist > 0 && dist <= range)
                    {
                        int row = dy + viewRadius;
                        int col = dx + viewRadius;
                        observations[5, row, col] = 1f;
                    }
                }
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
        if (LogObservations && agentEntity != null)
        {
            string[] channelNames = { "Allied Team", "Enemy Team", "Pickup", "Bush", "Wall/Bounds", "Attack Range" };
            string log = $"TacticalGridSensor - {agentEntity.name} (Team {agentEntity.TeamId}) - {viewSize}x{viewSize}\n";
            for (int c = 0; c < Channels; c++)
            {
                log += $"Channel {c} ({channelNames[c]}):\n";
                for (int r = viewSize - 1; r >= 0; r--)
                {
                    for (int w = 0; w < viewSize; w++)
                    {
                        log += observations[c, r, w] > 0.5f ? "# " : "_ ";
                    }
                    log += "\n";
                }
            }
            Debug.Log(log);
        }

        return viewSize * viewSize * Channels;
    }

    public void Update() { }
    public void Reset() { }
    public byte[] GetCompressedObservation() => null;
}
