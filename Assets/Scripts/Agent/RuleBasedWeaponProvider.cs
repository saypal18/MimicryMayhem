using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RuleBasedWeaponProvider : MonoBehaviour
{
    private List<Pickup> weaponPrefabs;
    private float spawnDelay;
    private SortedInventory inventory;
    private Grid grid;
    private Transform pickupParent;
    private Coroutine spawnCoroutine;
    private Entity entity;

    public void Initialize(List<Pickup> prefabs, float delay, SortedInventory inventory, Grid grid, Transform parent, Entity entity)
    {
        this.weaponPrefabs = prefabs;
        this.spawnDelay = delay;
        this.inventory = inventory;
        this.grid = grid;
        this.pickupParent = parent;
        this.entity = entity;
    }

    private void Update()
    {
        if (inventory == null || entity == null)
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
            return;
        }

        bool hasNoWeapon = !inventory.HasAnyItem();

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

        if (weaponPrefabs != null && weaponPrefabs.Count > 0)
        {
            Pickup prefab = weaponPrefabs[Random.Range(0, weaponPrefabs.Count)];
            Vector2Int pos = GetComponent<GridPlaceable>().Position;
            
            Pickup spawned = PoolingEntity.Spawn(prefab, pickupParent);
            spawned.Initialize(grid, pos, null);
        }

        spawnCoroutine = null;
    }
}
