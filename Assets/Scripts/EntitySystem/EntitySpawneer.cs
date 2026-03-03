using UnityEngine;
[System.Serializable]

public class EntitySpawner
{
    private Grid grid;
    private PickupPlacer pickupPlacer;
    private Transform entityParent;
    [SerializeField] private Vector2Int spawnPosition;
    [SerializeField] private Entity entity;
    public void Initialize(Grid grid, PickupPlacer pickupPlacer, Transform entityParent)
    {
        this.grid = grid;
        this.pickupPlacer = pickupPlacer;
        this.entityParent = entityParent;
    }
    public Entity Spawn()
    {
        Entity clonedEntity = GameObject.Instantiate(this.entity, grid.GetWorldPosition(spawnPosition), Quaternion.identity, entityParent);
        clonedEntity.Initialize(grid);

        if (clonedEntity.TryGetComponent(out PickupCollector collector))
        {
            collector.Initialize(grid, pickupPlacer, clonedEntity.gridMovement, clonedEntity.pickupHandler);
        }

        return clonedEntity;
    }
}