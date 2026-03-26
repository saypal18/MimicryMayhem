using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PickupPlacer
{
    [SerializeField] private List<Pickup> pickupPrefabs;
    [SerializeField] private Transform pickupParent;
    [SerializeField] private float spawnInterval = 2.0f;
    [Header("Pickup Settings")]
    [SerializeField] public float pickupPercentage = 25f;
    private Grid grid;
    private float timer;
    private readonly List<Pickup> activePickups = new List<Pickup>();

    /// <summary>Number of pickups currently active on the grid.</summary>
    public int ActivePickupCount => activePickups.Count;

    public void Initialize(Grid grid)
    {
        this.grid = grid;
        timer = 0;
        activePickups.Clear();
    }

    public void Tick(float deltaTime)
    {
        if (spawnInterval <= 0) return;

        timer += deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0;

            // Limit the total possible pickups to prevent memory bloat and infinite farming CPU stalls
            int maxPickups = Mathf.RoundToInt((grid.Size.x * grid.Size.y) * (pickupPercentage / 100f));
            if (ActivePickupCount < maxPickups)
            {
                SpawnAtRandomPositions(1);
            }
        }
    }
    public void SetInterval(float interval)
    {
        spawnInterval = interval;
    }

    public void SpawnAtPosition(Vector2Int position)
    {
        Pickup pickup = PoolingEntity.Spawn(pickupPrefabs[Random.Range(0, pickupPrefabs.Count)], pickupParent);
        pickup.Initialize(grid, position);

        activePickups.Add(pickup);
        if (pickup.TryGetComponent(out PoolingEntity poolingEntity))
        {
            poolingEntity.OnDespawning += CreateDespawnHandler(pickup, poolingEntity);
        }
    }

    public void DropItem(GameObject dropper, WeaponItem item, Vector2Int position)
    {
        if (pickupPrefabs.Count > 0)
        {
            // Use the first prefab as a base for custom drops
            Pickup pickup = PoolingEntity.Spawn(pickupPrefabs[0], pickupParent);
            if (pickup is WeaponPickup weaponPickup)
            {
                weaponPickup.SetItem(item);
            }
            pickup.Initialize(grid, position, dropper);
            activePickups.Add(pickup);
            if (pickup.TryGetComponent(out PoolingEntity poolingEntity))
            {
                poolingEntity.OnDespawning += CreateDespawnHandler(pickup, poolingEntity);
            }
        }
    }

    private System.Action CreateDespawnHandler(Pickup pickup, PoolingEntity poolingEntity)
    {
        System.Action handler = null;
        handler = () =>
        {
            activePickups.Remove(pickup);
            // poolingEntity.OnDespawning -= handler;
        };
        return handler;
    }

    public void SpawnInitialPickups(int totalArea)
    {
        int count = Mathf.RoundToInt(totalArea * (pickupPercentage / 100f));
        SpawnAtRandomPositions(count);
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