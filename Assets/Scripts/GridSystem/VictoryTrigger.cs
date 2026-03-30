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

    [Header("UI Reference")]
    [Tooltip("The Victory Panel GameObject to activate when the player reaches this tile with the key.")]
    public GameObject victoryPanel;

    public void Initialize(Grid grid, Vector2Int position, GameObject victoryPanel)
    {
        this.victoryPanel = victoryPanel;
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
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            Debug.Log("[VictoryTrigger] Player reached victory tile with boss key. VICTORY!");
        }
        else
        {
            Debug.LogWarning("[VictoryTrigger] Victory! (But Victory Panel is not assigned in the inspector)");
        }
    }
}
