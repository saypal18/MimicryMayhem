using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PickupPlacer
{
    private Grid grid;
    private Transform parent;
    [SerializeField] private GrowPickup growPickup;
    private Dictionary<Vector2Int, GrowPickup> pickables = new();
    public void Initialize(Grid grid, Transform parent)
    {
        this.grid = grid;
        this.parent = parent;
    }
    public void Place(Vector2Int gridPosition)
    {
        GrowPickup growPickup = PoolingEntity.Spawn(this.growPickup, grid.GetWorldPosition(gridPosition), Quaternion.identity, parent);
        growPickup.Setup(this, gridPosition);
        this.pickables.Add(gridPosition, growPickup);
    }

    public void RandomPlacement(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2Int gridPosition;
            do
            {
                gridPosition = new Vector2Int(Random.Range(0, grid.Size.x), Random.Range(0, grid.Size.y));

            } while (pickables.ContainsKey(gridPosition));
            Place(gridPosition);
        }
    }
    public void Remove(Vector2Int gridPosition)
    {
        if (pickables.ContainsKey(gridPosition))
        {
            pickables.Remove(gridPosition);
        }
    }
    public List<Vector2Int> GetPickupPositions()
    {
        return new List<Vector2Int>(pickables.Keys);
    }
}