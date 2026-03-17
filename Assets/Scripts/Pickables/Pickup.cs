using UnityEngine;

public abstract class Pickup : MonoBehaviour
{
    public abstract bool Collected(GameObject picker);

    [SerializeField] private GridPlaceable gridPlaceable;
    public virtual void Initialize(Grid grid, Vector2Int position)
    {
        gridPlaceable.Initialize(grid, position);
    }
}