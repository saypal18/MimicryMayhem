using Unity.MLAgents.Sensors;
using UnityEngine;

public enum GridSensorType
{
    Custom,
    Easy,
    Tactical
}

/// <summary>
/// Attach this SensorComponent to the agent prefab alongside SurvivorAgent.
/// Grid and agent-specific references are injected at runtime by SurvivorAgent.Initialize()
/// — no scene-object references need to be set in the Inspector.
/// </summary>
public class CustomGridSensorComponent : SensorComponent
{
    private ISensor sensor;

    // Pending refs if SetAgentReferences is called before CreateSensors
    private GridPlaceable pendingPlaceable;
    private DamageDealer pendingDamageResolver;
    private Grid pendingGrid;
    private EntitySpawner pendingSpawner;
    private Entity pendingAgentEntity;

    [Header("Settings")]
    public GridSensorType sensorType = GridSensorType.Custom;
    [SerializeField] private int viewRadius = 5;

    [Header("Debug")]
    public bool showGizmos = true;
    public bool logObservations = false;

    public override ISensor[] CreateSensors()
    {
        // Create sensor with null grid — it will be set via SetAgentReferences
        if (sensorType == GridSensorType.Custom)
        {
            sensor = new CustomGridSensor(null, viewRadius);
        }
        else if (sensorType == GridSensorType.Tactical)
        {
            var tacticalSensor = new TacticalGridSensor(null, viewRadius, null, null);
            tacticalSensor.LogObservations = logObservations;
            sensor = tacticalSensor;
        }

        // Apply any references that arrived before CreateSensors was called
        if (pendingPlaceable != null)
        {
            SetSensorReferences(pendingPlaceable, pendingDamageResolver, pendingGrid, pendingSpawner, pendingAgentEntity);
        }

        return new ISensor[] { sensor };
    }

    /// <summary>
    /// Called by SurvivorAgent.Initialize() to inject the runtime grid and per-agent refs.
    /// </summary>
    public void SetAgentReferences(GridPlaceable agentPlaceable, DamageDealer agentDamageResolver, Grid grid, EntitySpawner spawner, Entity agentEntity)
    {
        if (sensor != null)
        {
            SetSensorReferences(agentPlaceable, agentDamageResolver, grid, spawner, agentEntity);
        }
        else
        {
            // CreateSensors hasn't fired yet — store and apply when it does
            pendingPlaceable = agentPlaceable;
            pendingDamageResolver = agentDamageResolver;
            pendingGrid = grid;
            pendingSpawner = spawner;
            pendingAgentEntity = agentEntity;
        }
    }

    private void SetSensorReferences(GridPlaceable placeable, DamageDealer damageResolver, Grid grid, EntitySpawner spawner, Entity agentEntity)
    {
        if (sensor is CustomGridSensor customSensor)
        {
            customSensor.SetAgentReferences(placeable, damageResolver, grid);
        }
        else if (sensor is EasyGridSensor easySensor)
        {
            easySensor.SetAgentReferences(placeable, grid);
        }
        else if (sensor is TacticalGridSensor tacticalSensor)
        {
            tacticalSensor.SetAgentReferences(placeable, grid, spawner, agentEntity);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        if (Application.isPlaying)
        {
            if (sensor != null)
            {
                if (sensor is CustomGridSensor customSensor)
                {
                    customSensor.OnDrawGizmos();
                }
                else if (sensor is TacticalGridSensor tacticalSensor)
                {
                    tacticalSensor.OnDrawGizmos();
                }
            }
            else if (pendingPlaceable != null)
            {
                // We have references but ML-Agents hasn't called CreateSensors yet
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
#if UNITY_EDITOR
                UnityEditor.Handles.Label(transform.position + Vector3.up, "Waiting for ML-Agents Sensor Creation...");
#endif
            }
            else
            {
                // No references yet
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, 0.3f);
#if UNITY_EDITOR
                UnityEditor.Handles.Label(transform.position + Vector3.up, "Missing Grid/Agent References!");
#endif
            }
        }
        else
        {
            DrawEditModePreview();
        }
    }

    private void DrawEditModePreview()
    {
        var initializer = FindFirstObjectByType<GameInitializer>();
        if (initializer != null)
        {
            Gizmos.color = new Color(1, 1, 1, 0.2f);
            int size = viewRadius * 2 + 1;
            Gizmos.DrawWireCube(transform.position, new Vector3(size, size, 0.1f));
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up, "Sensor Preview (Ego-centric)");
#endif
        }
    }

    // private void Update()
    // {
    //     if (sensor is EasyGridSensor easySensor)
    //     {
    //         easySensor.LogObservations = logObservations;
    //     }
    // }
}
