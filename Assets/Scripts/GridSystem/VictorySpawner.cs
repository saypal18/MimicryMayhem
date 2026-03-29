using UnityEngine;

/// <summary>
/// Modular spawner for the Victory Trigger tile.
/// Following the same pattern as BossCreator, this component should be attached
/// to a GameInitializer's GameObject to enable the victory condition for that environment.
/// </summary>
public class VictorySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private VictoryTrigger victoryTriggerPrefab;
    [SerializeField] private Vector2Int victoryTriggerPosition;
    
    [Header("UI Reference")]
    [Tooltip("Reference to the Victory Panel UI in the scene.")]
    [SerializeField] private GameObject victoryPanel;

    private Grid grid;

    public void Initialize(Grid grid)
    {
        this.grid = grid;
    }

    /// <summary>
    /// Spawns the victory trigger at the pre-specified position.
    /// Returns the spawned GameObject for cleanup.
    /// </summary>
    public GameObject SpawnVictoryTrigger()
    {
        if (victoryTriggerPrefab == null || grid == null)
        {
            Debug.LogWarning("[VictorySpawner] Required references not set!");
            return null;
        }

        Vector3 worldPos = grid.GetWorldPosition(victoryTriggerPosition);
        GameObject triggerObj = PoolingEntity.Spawn(victoryTriggerPrefab.gameObject, worldPos, Quaternion.identity, transform);
        
        VictoryTrigger trigger = triggerObj.GetComponent<VictoryTrigger>();
        if (trigger != null)
        {
            trigger.Initialize(grid, victoryTriggerPosition, victoryPanel);
            Debug.Log($"[VictorySpawner] Victory trigger spawned at {victoryTriggerPosition}.");
        }

        return triggerObj;
    }
}
