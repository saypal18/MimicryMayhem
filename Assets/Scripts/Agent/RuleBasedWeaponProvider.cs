using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RuleBasedWeaponProvider : MonoBehaviour
{
    private List<Pickup> fullPool;
    private List<Pickup> currentPool;
    private float spawnDelay;
    private SortedInventory inventory;
    private Grid grid;
    private Transform pickupParent;
    private Coroutine spawnCoroutine;
    private Entity entity;

    public void Initialize(List<Pickup> fullPool, List<Pickup> currentPool, float delay, SortedInventory inventory, Grid grid, Transform parent, Entity entity)
    {
        this.fullPool = fullPool;
        this.currentPool = currentPool;
        this.spawnDelay = delay;
        this.inventory = inventory;
        this.grid = grid;
        this.pickupParent = parent;
        this.entity = entity;

        if (!entity.IsPlayer && !inventory.HasAnyItem())
        {
            SpawnWeapon();
        }
    }

    private void Update()
    {
        if (inventory == null || entity == null || fullPool == null)
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
            return;
        }

        bool hasNoWeapon = !inventory.HasAnyItem() && !entity.IsPlayer;

        if (hasNoWeapon && spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnWeaponRoutine());
        }
        else if (!hasNoWeapon && spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnWeaponRoutine()
    {
        yield return new WaitForSeconds(spawnDelay);
        SpawnWeapon();
        spawnCoroutine = null;
    }

    private void SpawnWeapon()
    {
        if (fullPool == null || fullPool.Count == 0 || currentPool == null) return;

        if (currentPool.Count == 0)
        {
            currentPool.AddRange(fullPool);
        }

        int index = Random.Range(0, currentPool.Count);
        Pickup prefab = currentPool[index];
        currentPool.RemoveAt(index);

        Vector2Int pos = GetComponent<GridPlaceable>().Position;
        Pickup spawned = PoolingEntity.Spawn(prefab, pickupParent);
        spawned.Initialize(grid, pos, null);
    }
}
