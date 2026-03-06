using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// Attach this SensorComponent to the agent prefab alongside SurvivorAgent.
/// Grid and agent-specific references are injected at runtime by SurvivorAgent.Initialize()
/// — no scene-object references need to be set in the Inspector.
/// </summary>
public class CustomGridSensorComponent : SensorComponent
{
    private CustomGridSensor sensor;

    // Pending refs if SetAgentReferences is called before CreateSensors
    private GridPlaceable pendingPlaceable;
    private DamageResolver pendingDamageResolver;
    private Grid pendingGrid;

    [Header("Settings")]
    [SerializeField] private int viewRadius = 5;

    [Header("Debug")]
    public bool showGizmos = true;

    public override ISensor[] CreateSensors()
    {
        // Create sensor with null grid — it will be set via SetAgentReferences
        sensor = new CustomGridSensor(null, viewRadius);

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

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        if (Application.isPlaying)
        {
            if (sensor != null)
            {
                sensor.OnDrawGizmos();
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
}
