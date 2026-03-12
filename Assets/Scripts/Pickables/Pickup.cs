using UnityEngine;

public abstract class Pickup : MonoBehaviour
{
    public abstract void Collected(GameObject picker);

    [SerializeField] private GridPlaceable gridPlaceable;
    public void Initialize(Grid grid, Vector2Int position)
    {
        gridPlaceable.Initialize(grid, position);
    }
}