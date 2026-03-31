using UnityEngine;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// Attach this script to your pre-specified Victory Tile/Door.
/// It detects the player via OnTriggerEnter2D and checks for the Boss Key flag.
/// NO FindObject calls are used; assign the Victory Panel in the inspector.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class VictoryTrigger : MonoBehaviour
{
    [SerializeField] private GridPlaceable gridPlaceable;
    public GridPlaceable Placeable => gridPlaceable;

    [Header("Victory Sequence")]
    [SerializeField] private VictoryAnimationController animationController;

    [Header("Audio")]
    [SerializeField] private EventReference victoryStingerSoundEvent;
    private SoundManager soundManager; // Set at runtime via Initialize()

    public void Initialize(Grid grid, Vector2Int position, VictoryAnimationController controller, SoundManager soundManager)
    {
        this.animationController = controller;
        this.soundManager = soundManager;
        if (gridPlaceable == null)
        {
            gridPlaceable = GetComponent<GridPlaceable>();
        }
        gridPlaceable.Type = GridPlaceable.PlaceableType.Victory;
        gridPlaceable.Initialize(grid, position);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the entering object is an Entity
        if (other.TryGetComponent(out Entity entity))
        {
            // Verify it's the player and they have the key
            if (entity.IsPlayer && entity.HasBossKey)
            {
                TriggerVictory();
            }
        }
    }

    private void TriggerVictory()
    {
        PlayVictoryStinger();

        if (animationController != null)
        {
            // Focus on the first child if it exists, otherwise the door itself.
            Transform focalTarget = transform.childCount > 0 ? transform.GetChild(0) : transform;

            animationController.PlayVictoryAnimation(focalTarget);
            Debug.Log($"[VictoryTrigger] Victory sequence triggered! Focusing on: {focalTarget.name}");
        }
        else
        {
            Debug.LogWarning("[VictoryTrigger] Victory! (But VictoryAnimationController is not assigned)");
        }
    }

    private void PlayVictoryStinger()
    {
        if (Trainer.IsTraining) return;
        if (victoryStingerSoundEvent.IsNull) return;

        EventInstance instance = RuntimeManager.CreateInstance(victoryStingerSoundEvent);
        // We're going to stop the music and ambience from FMOD when this event
        // is started.
        instance.start();
        instance.release();
    }
}
