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
    [SerializeField] Entity entity;
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
    public bool logObservations = false;

    public override ISensor[] CreateSensors()
    {
        // Debug.Log("creating sensor");
        // Create sensor with null grid — it will be set via SetAgentReferences
        if (sensorType == GridSensorType.Custom)
        {
            sensor = new CustomGridSensor(null, viewRadius);
        }
        else if (sensorType == GridSensorType.Easy)
        {
            var easySensor = new EasyGridSensor(null, viewRadius);
            easySensor.LogObservations = logObservations;
            sensor = easySensor;
        }
        else if (sensorType == GridSensorType.Tactical)
        {
            var tacticalSensor = new TacticalGridSensor(viewRadius, entity);
            //var tacticalSensor = new TacticalGridSensor(entity.CurrentGrid, entity.gridPlaceable, viewRadius, null, entity);
            tacticalSensor.LogObservations = logObservations;
            sensor = tacticalSensor;
        }

        // Apply any references that arrived before CreateSensors was called
        // if (pendingPlaceable != null)
        // {
        //     SetSensorReferences(pendingPlaceable, pendingDamageResolver, pendingGrid, pendingSpawner, pendingAgentEntity);
        // }

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
            tacticalSensor.SetAgentReferences(agentEntity);
        }
    }

    private void Update()
    {
        //if (logObservations)
        //{
        //    Debug.Log("here");
        //}
        if (sensor == null) return;

        if (sensor is CustomGridSensor custom)
            custom.LogObservations = logObservations;
        else if (sensor is EasyGridSensor easy)
            easy.LogObservations = logObservations;
        else if (sensor is TacticalGridSensor tactical)
            tactical.LogObservations = logObservations;
    }
}
