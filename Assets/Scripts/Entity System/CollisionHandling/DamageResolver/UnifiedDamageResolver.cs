using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class UnifiedDamageResolver
{
    private float cooldownTime = 1;
    private Dictionary<DamageDealer, float> damageCooldowns = new();
    GridPlaceable gridPlaceable;
    SortedInventory inventory;
    public void Initialize(CollisionResolver collisionResolver, SortedInventory inventory)
    {
        collisionResolver.OnCollision += OnCollision;
        gridPlaceable = collisionResolver.GetComponent<GridPlaceable>();
        this.inventory = inventory;
    }
    public void OnCollision(GameObject other)
    {
        if (other.TryGetComponent(out DamageDealer damageDealer))
        {
            if (damageCooldowns.ContainsKey(damageDealer) && damageCooldowns[damageDealer] > Time.time - cooldownTime) return;
            damageCooldowns[damageDealer] = Time.time;
            Debug.Log("Damage of tier " + damageDealer.tier + " dealt in direction " + damageDealer.direction + " To tier: " + inventory.highestTier);
            gridPlaceable.Move(damageDealer.direction);
        }
    }
}