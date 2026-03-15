using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
[System.Serializable]
public class UnifiedDamageResolver
{
    [SerializeField] private float cooldownTime = 1;
    public Action OnDamageTaken;

    private Dictionary<DamageDealer, float> damageCooldowns = new();
    GridPlaceable gridPlaceable;
    SortedInventory inventory;

    EquippedItem equippedItem;
    private IEntityMovement knockbackMovement;
    private AbilityController controller;
    [SerializeField] private float knockbackTime = 0.1f;
    [SerializeField] private int knockbackBlocks = 1;
    public void Initialize(CollisionResolver collisionResolver, SortedInventory inventory, EquippedItem equippedItem, EntityMovementFactory movementFactory, AbilityController controller)
    {
        collisionResolver.OnCollision += OnCollision;
        gridPlaceable = collisionResolver.GetComponent<GridPlaceable>();
        this.inventory = inventory;
        this.equippedItem = equippedItem;
        OnDamageTaken = null;
        knockbackMovement = movementFactory.GetMovement(this.GetType());
        knockbackMovement.Initialize(knockbackTime, knockbackBlocks, gridPlaceable);
        this.controller = controller;
    }
    public void OnCollision(GameObject other)
    {
        if (other != null && other.TryGetComponent(out DamageDealer damageDealer))
        {
            if (damageCooldowns.ContainsKey(damageDealer) && damageCooldowns[damageDealer] > Time.time - cooldownTime) return;
            damageCooldowns[damageDealer] = Time.time;
            AcceptDamage(damageDealer);
            //Debug.Log("Damage of tier " + damageDealer.tier + " dealt in direction " + damageDealer.direction + " To tier: " + inventory.highestTier);
        }
    }

    public void AcceptDamage(DamageDealer damageDealer)
    {
        InventoryItem item = equippedItem.Get();
        controller.Control();
        if (item == null || !(item is WeaponItem))
        {
            return;
        }


        damageDealer.OnDamageDealt?.Invoke();
        OnDamageTaken?.Invoke();
        if (damageDealer.applyKnockback)
        {
            knockbackMovement.Move(damageDealer.direction);
        }
        WeaponItem weaponItem = (WeaponItem)item;
        // if grip is < attacker tier, then weapon switches to attacker possession, grip is reset check WeaponPickup for more details
        // if grip is >= attacker tier, then decrease grip by 1

        if (weaponItem.currentGrip < damageDealer.tier)
        {
            Entity attackerEntity = damageDealer.GetComponentInParent<Entity>();
            if (attackerEntity != null)
            {
                weaponItem.Initialize();
                attackerEntity.inventory.AddItem(weaponItem, 1);
            }
            inventory.GetSlot(equippedItem.GetIndex()).Remove();
            inventory.ShiftUp();

            //foreach (var slot in inventory.GetSlots())
            //{
            //    if (slot.item == weaponItem)
            //    {
            //        slot.Remove();
            //        break;
            //    }
            //}
            //inventory.UpdateHighestTier();
        }
        else
        {
            weaponItem.currentGrip--;
        }
    }
}