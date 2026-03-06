using UnityEngine;

public class GrowPickup : MonoBehaviour, Pickup
{
    [SerializeField] private GridPlaceable gridPlaceable;
    public void Initialize(Grid grid, Vector2Int position)
    {
        gridPlaceable.Initialize(grid, position);
    }
    public void Collected()
    {
        PoolingEntity.Despawn(gameObject);
    }

}