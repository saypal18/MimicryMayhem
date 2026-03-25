using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using FMODUnity;
using FMOD.Studio;
[System.Serializable]
public class UnifiedDamageResolver
{
    public Action<Entity> OnDamageTaken;

    GridPlaceable gridPlaceable;
    SortedInventory inventory;

    EquippedItem equippedItem;
    private IEntityMovement knockbackMovement;
    private AbilityController controller;
    private MoveInfo moveInfo;
    [SerializeField] private float knockbackTime = 0.1f;
    [SerializeField] private int knockbackBlocks = 1;

    [Header("Audio")]
    [SerializeField] private EventReference weaponImpactSoundEvent;
    [SerializeField] private EventReference gripReducedSoundEvent;
    public void Initialize(CollisionResolver collisionResolver, SortedInventory inventory, EquippedItem equippedItem, EntityMovementFactory movementFactory, AbilityController controller, MoveInfo moveInfo)
    {
        // collisionResolver.OnCollision += OnCollision; // Removed action-subscriber
        gridPlaceable = collisionResolver.GetComponent<GridPlaceable>();
        this.inventory = inventory;
        this.equippedItem = equippedItem;
        this.moveInfo = moveInfo;
        OnDamageTaken = null;
        knockbackMovement = movementFactory.GetMovement(this.GetType());
        knockbackMovement.Initialize(knockbackTime, knockbackBlocks, gridPlaceable, moveInfo);
        this.controller = controller;
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

        PlayImpactSound(damageDealer);

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
        }
        else
        {
            weaponItem.currentGrip--;
            PlayGripReducedSound(weaponItem);
        }
    }

    private void PlayImpactSound(DamageDealer damageDealer)
    {
        if (weaponImpactSoundEvent.IsNull) return;

        Entity attacker = damageDealer.GetComponentInParent<Entity>();
        InventoryItem attackerItem = attacker != null ? attacker.equippedItem.Get() : null;

        EventInstance instance = RuntimeManager.CreateInstance(weaponImpactSoundEvent);
        if (attackerItem != null)
        {
            instance.setParameterByNameWithLabel("ItemType", attackerItem.itemType.ToString());
        }
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(gridPlaceable.transform.position));
        instance.start();
        instance.release();
    }

    private void PlayGripReducedSound(WeaponItem weaponItem)
    {
        if (gripReducedSoundEvent.IsNull) return;

        EventInstance instance = RuntimeManager.CreateInstance(gripReducedSoundEvent);
        instance.setParameterByName("GripAmount", weaponItem.currentGrip);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(gridPlaceable.transform.position));
        instance.start();
        instance.release();
    }

}