using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PickupPlacer
{
    [SerializeField] private GrowPickup pickupPrefab;
    [SerializeField] private Transform pickupParent;
    [SerializeField] private float spawnInterval = 2.0f;
    private Grid grid;
    private float timer;

    public void Initialize(Grid grid)
    {
        this.grid = grid;
        timer = 0;
    }

    public void Tick(float deltaTime)
    {
        if (spawnInterval <= 0) return;

        timer += deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0;
            SpawnAtRandomPositions(1);
        }
    }

    public void SetInterval(float interval)
    {
        spawnInterval = interval;
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