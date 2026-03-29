using UnityEngine;

public abstract class Pickup : MonoBehaviour
{
    protected GameObject dropper;
    public bool WasDroppedByEntity => dropper != null;
    public abstract bool Collected(GameObject picker);
    public virtual void Initialize(Grid grid, Vector2Int position, GameObject dropper)
    {
        Initialize(grid, position);
        this.dropper = dropper;
    }

    [SerializeField] private GridPlaceable gridPlaceable;
    public virtual void Initialize(Grid grid, Vector2Int position)
    {
        gridPlaceable.Initialize(grid, position);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (dropper == null) return;
        
        bool isDropper = other.gameObject == dropper;
        if (!isDropper && other.TryGetComponent(out Root root))
        {
            isDropper = root.GO == dropper;
        }

        if (isDropper)
        {
            dropper = null;
        }
    }
}