using UnityEngine;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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
    
    [Header("Sequence Reference")]
    [Tooltip("Reference to the Victory Animation Controller in the scene.")]
    [SerializeField] private VictoryAnimationController animationController;

    private Grid grid;
    private SoundManager soundManager;

    public void Initialize(Grid grid, VictoryAnimationController controller, SoundManager soundManager)
    {
        this.grid = grid;
        this.animationController = controller;
        this.soundManager = soundManager;
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
            trigger.Initialize(grid, victoryTriggerPosition, animationController, soundManager);
            Debug.Log($"[VictorySpawner] Victory trigger spawned at {victoryTriggerPosition}.");
        }

        return triggerObj;
    }
}
