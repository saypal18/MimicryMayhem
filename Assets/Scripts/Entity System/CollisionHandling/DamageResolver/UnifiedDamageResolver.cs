using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
[System.Serializable]
public class UnifiedDamageResolver
{
    public Action<Entity> OnDamageTaken;

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
            if (damageDealer.TryRegisterHit(gridPlaceable.Entity))
            {
                AcceptDamage(damageDealer);
            }
        }
    }

    public void AcceptDamage(DamageDealer damageDealer)
    {
        InventoryItem item = equippedItem.Get();
        if (item == null || !(item is WeaponItem))
        {
            return;
        }


        damageDealer.OnDamageDealt?.Invoke(gridPlaceable.Entity);
        OnDamageTaken?.Invoke(damageDealer.entity);
        if (damageDealer.applyKnockback)
        {
            controller.Control(1);
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