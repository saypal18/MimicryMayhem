using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PickupPlacer
{
    [SerializeField] private GrowPickup pickupPrefab;
    [SerializeField] private Transform pickupParent;
    private Grid grid;

    public void Initialize(Grid grid)
    {
        this.grid = grid;
    }

    public void SpawnAtPosition(Vector2Int position)
    {
        GrowPickup pickup = PoolingEntity.Spawn(pickupPrefab, pickupParent);
        pickup.Initialize(grid, position);
    }

    public void SpawnAtRandomPositions(int count)
    {
        List<Vector2Int> randomPositions = grid.GetRandomEmptyPositions(count);
        foreach (Vector2Int randomPosition in randomPositions)
        {
            SpawnAtPosition(randomPosition);
        }
    }
}