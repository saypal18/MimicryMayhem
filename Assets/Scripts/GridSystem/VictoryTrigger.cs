using UnityEngine;

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

    public void Initialize(Grid grid, Vector2Int position, VictoryAnimationController controller)
    {
        this.animationController = controller;
        if (gridPlaceable == null)
        {
            gridPlaceable = GetComponent<GridPlaceable>();
        }
        gridPlaceable.Type = GridPlaceable.PlaceableType.Victory;
        gridPlaceable.Initialize(grid, position);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Entity entity))
        {
            if (entity.IsPlayer && entity.HasBossKey)
            {
                TriggerVictory();
            }
        }
    }

    private void TriggerVictory()
    {
        if (SoundManager.Events != null)
            SoundManager.PlayOneShot(SoundManager.Events.levelCompleted);

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
}
