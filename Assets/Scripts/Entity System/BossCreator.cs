using UnityEngine;
using System.Collections.Generic;

public class BossCreator : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private Entity bossPrefab;
    [SerializeField] private int bossInventorySlotCount = 10;
    [SerializeField] private List<WeaponItem> preDeterminedItems;
    [SerializeField] private GameObject keyPickupPrefab;
    [SerializeField] private int bossTeamId = 99; // Default to a unique team ID

    private Grid grid;
    private EntitySpawner entitySpawner;
    private PickupPlacer pickupPlacer;

    public void Initialize(Grid grid, EntitySpawner spawner, PickupPlacer pickupPlacer)
    {
        this.grid = grid;
        this.entitySpawner = spawner;
        this.pickupPlacer = pickupPlacer;
    }

    public void SpawnBoss()
    {
        if (bossPrefab == null || grid == null || entitySpawner == null)
        {
            Debug.LogWarning("[BossCreator] BossPrefab or references not set!");
            return;
        }

        Vector2Int? nullablePos = grid.GetRandomEmptyPosition();
        if (!nullablePos.HasValue)
        {
            Debug.LogWarning("[BossCreator] No empty position found for boss!");
            return;
        }

        Vector2Int spawnPos = nullablePos.Value;
        
        // Use the refactored SpawnAtPosition to spawn the boss
        // Passing initializeWeaponProvider = false to satisfy the "items won't spawn on its place" requirement
        int defaultInventorySize = bossPrefab.inventory != null ? bossPrefab.inventory.slotCount : 0;
        Entity boss = entitySpawner.SpawnAtPosition(bossPrefab, spawnPos, bossTeamId, false, bossInventorySlotCount, defaultInventorySize);



        if (boss != null)
        {
            boss.IsBoss = true;
            boss.canBeStunned = false;
            boss.HasBossKey = true;
            // Add pre-determined items
            if (boss.inventory != null)
            {
                foreach (var item in preDeterminedItems)
                {
                    if (item != null)
                    {
                        boss.inventory.AddItem(item, 1);
                    }
                }
            }

            // Setup drop on kill (requirement: only drops if killed by an attack, not on env reset)
            if (boss.damageResolver != null)
            {
                // Capture the boss and its position for the drop
                boss.damageResolver.OnKilled += (attacker) => HandleBossKilled(boss.gameObject, boss.Position);
            }
        }
    }

    private void HandleBossKilled(GameObject bossObj, Vector2Int lastPosition)
    {
        if (keyPickupPrefab != null)
        {
            // Spawn the key at the boss's last position
            Vector3 worldPos = grid.GetWorldPosition(lastPosition);
            GameObject keyObj = PoolingEntity.Spawn(keyPickupPrefab, worldPos, Quaternion.identity, transform);
            
            if (keyObj.TryGetComponent(out Pickup keyPickup))
            {
                keyPickup.Initialize(grid, lastPosition, bossObj);
                Debug.Log($"[BossCreator] Boss killed at {lastPosition}. Spawning key.");
            }
        }
    }
}

