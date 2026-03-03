using UnityEngine;

public class GrowPickup : MonoBehaviour, IPickable
{
    private PickupPlacer placer;
    private Vector2Int gridPosition;

    public void Setup(PickupPlacer placer, Vector2Int position)
    {
        this.placer = placer;
        this.gridPosition = position;
    }

    public void PickUp(Entity entity)
    {
        entity.pickupHandler.Pickup(this);
        placer?.Remove(gridPosition);

        if (entity.TryGetComponent(out PickupCollector collector))
        {
            collector.OnPickupCollected();
        }

        PoolingEntity.Despawn(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Root>(out Root root))
        {
            PickUp(root.GO.GetComponent<Entity>());
        }
    }
}