// using Unity.MLAgents.Sensors;
// using UnityEngine;

// /// <summary>
// /// Attach this SensorComponent to the agent prefab alongside SurvivorAgent/AttackerAgent.
// /// Grid and agent-specific references are injected at runtime by Agent Initialize.
// /// </summary>
// public class EasyGridSensorComponent : SensorComponent
// {
//     private EasyGridSensor sensor;

//     private GridPlaceable pendingPlaceable;
//     private Grid pendingGrid;

//     [Header("Settings")]
//     [SerializeField] private int viewRadius = 5;

//     [Header("Debug")]
//     public bool showGizmos = true;

//     public override ISensor[] CreateSensors()
//     {
//         sensor = new EasyGridSensor(null, viewRadius);

//         if (pendingPlaceable != null)
//         {
//             sensor.SetAgentReferences(pendingPlaceable, pendingGrid);
//         }

//         return new ISensor[] { sensor };
//     }

//     public void SetAgentReferences(GridPlaceable agentPlaceable, Grid grid)
//     {
//         if (sensor != null)
//         {
//             sensor.SetAgentReferences(agentPlaceable, grid);
//         }
//         else
//         {
//             pendingPlaceable = agentPlaceable;
//             pendingGrid = grid;
//         }
//     }

//     private void OnDrawGizmosSelected()
//     {
//         if (!showGizmos) return;

//         if (Application.isPlaying)
//         {
//             if (sensor != null)
//             {
//                 sensor.OnDrawGizmos();
//             }
//             else if (pendingPlaceable != null)
//             {
//                 // We have references but ML-Agents hasn't called CreateSensors yet
//                 Gizmos.color = Color.cyan;
//                 Gizmos.DrawWireSphere(transform.position, 0.5f);
// #if UNITY_EDITOR
//                 UnityEditor.Handles.Label(transform.position + Vector3.up, "Waiting for ML-Agents Sensor Creation...");
// #endif
//             }
//             else
//             {
//                 // No references yet
//                 Gizmos.color = Color.red;
//                 Gizmos.DrawWireSphere(transform.position, 0.3f);
// #if UNITY_EDITOR
//                 UnityEditor.Handles.Label(transform.position + Vector3.up, "Missing Grid/Agent References!");
// #endif
//             }
//         }
//         else
//         {
//             DrawEditModePreview();
//         }
//     }

//     private void DrawEditModePreview()
//     {
//         var initializer = FindFirstObjectByType<GameInitializer>();
//         if (initializer != null)
//         {
//             Gizmos.color = new Color(1, 1, 1, 0.2f);
//             int size = viewRadius * 2 + 1;
//             Gizmos.DrawWireCube(transform.position, new Vector3(size, size, 0.1f));
// #if UNITY_EDITOR
//             UnityEditor.Handles.Label(transform.position + Vector3.up, "Sensor Preview (Ego-centric)");
// #endif
//         }
//     }
// }
