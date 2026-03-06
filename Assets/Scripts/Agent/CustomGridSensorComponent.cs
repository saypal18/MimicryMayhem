using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// Attach this SensorComponent to the agent prefab alongside SurvivorAgent.
/// Grid and agent-specific references are injected at runtime by SurvivorAgent.Initialize()
/// — no scene-object references need to be set in the Inspector.
/// </summary>
public class CustomGridSensorComponent : SensorComponent
{
    [Tooltip("Minimum power ratio (enemy/self) mapped to 0. Default 0.1")]
    [SerializeField] private float minRatio = 0.1f;

    [Tooltip("Maximum power ratio (enemy/self) mapped to 1. Default 10")]
    [SerializeField] private float maxRatio = 10f;

    private CustomGridSensor sensor;

    // Pending refs if SetAgentReferences is called before CreateSensors
    private GridPlaceable pendingPlaceable;
    private DamageResolver pendingDamageResolver;
    private Grid pendingGrid;

    public override ISensor[] CreateSensors()
    {
        // Create sensor with null grid — it will be set via SetAgentReferences
        sensor = new CustomGridSensor(null, minRatio, maxRatio);

        // Apply any references that arrived before CreateSensors was called
        if (pendingPlaceable != null)
        {
            sensor.SetAgentReferences(pendingPlaceable, pendingDamageResolver, pendingGrid);
            pendingPlaceable = null;
            pendingDamageResolver = null;
            pendingGrid = null;
        }

        return new ISensor[] { sensor };
    }

    /// <summary>
    /// Called by SurvivorAgent.Initialize() to inject the runtime grid and per-agent refs.
    /// </summary>
    public void SetAgentReferences(GridPlaceable agentPlaceable, DamageResolver agentDamageResolver, Grid grid)
    {
        if (sensor != null)
        {
            sensor.SetAgentReferences(agentPlaceable, agentDamageResolver, grid);
        }
        else
        {
            // CreateSensors hasn't fired yet — store and apply when it does
            pendingPlaceable = agentPlaceable;
            pendingDamageResolver = agentDamageResolver;
            pendingGrid = grid;
        }
    }
}
