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
    public Action<Entity> OnKilled;

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
    [SerializeField] private EventReference entityDeathSoundEvent;
    public void Initialize(CollisionResolver collisionResolver, SortedInventory inventory, EquippedItem equippedItem, EntityMovementFactory movementFactory, AbilityController controller, MoveInfo moveInfo)
    {
        // collisionResolver.OnCollision += OnCollision; // Removed action-subscriber
        gridPlaceable = collisionResolver.GetComponent<GridPlaceable>();
        this.inventory = inventory;
        this.equippedItem = equippedItem;
        this.moveInfo = moveInfo;
        OnDamageTaken = null;
        OnKilled = null;
        knockbackMovement = movementFactory.GetMovement(this.GetType());
        knockbackMovement.Initialize(knockbackTime, knockbackBlocks, gridPlaceable, moveInfo);
        this.controller = controller;
    }

    public void AcceptDamage(DamageDealer damageDealer)
    {
        damageDealer.OnDamageDealt?.Invoke(gridPlaceable.Entity);
        OnDamageTaken?.Invoke(damageDealer.entity);

        PlayImpactSound(damageDealer);

        Entity victim = gridPlaceable.Entity;
        if (victim != null && victim.IsKillable && !inventory.HasAnyItem())
        {
            // Rule-based enemies with no weapons are mimics and cannot be killed.
            if (victim.agent != null && victim.agent.isRuleBased)
            {
                return;
            }

            damageDealer.OnKillDealt?.Invoke(victim);
            OnKilled?.Invoke(damageDealer.entity);

            // Reward attacker with a grip reset on all weapons
            if (damageDealer.entity != null && damageDealer.entity.inventory != null)
            {
                damageDealer.entity.inventory.ResetAllGrips();
            }

            PlayDeathSound();
            PoolingEntity.Despawn(victim.gameObject);
            return;
        }

        InventoryItem item = equippedItem.Get();
        if (item == null || !(item is WeaponItem))
        {
            return;
        }

        WeaponItem weaponItem = (WeaponItem)item;
        // if grip is < attacker tier, then weapon switches to attacker possession, grip is reset check WeaponPickup for more details
        // if grip is >= attacker tier, then decrease grip by 1

        if (weaponItem.currentGrip < damageDealer.tier)
        {
            Entity victimEntity = gridPlaceable.Entity;
            Entity attackerEntity = damageDealer.entity;
            if (victimEntity != null && attackerEntity != null)
            {
                weaponItem.currentGrip = weaponItem.tier;
                victimEntity.OnDropItemToGrid?.Invoke(victimEntity, weaponItem, damageDealer.attackStartPosition);
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

    private string GetCharacterTypeLabel()
    {
        Entity self = gridPlaceable.Entity;
        return (self != null && self.IsPlayer) ? "Player" : "Enemy";
    }

    private void PlayImpactSound(DamageDealer damageDealer)
    {
        if (Trainer.IsTraining) return;
        if (weaponImpactSoundEvent.IsNull) return;

        Entity attacker = damageDealer.GetComponentInParent<Entity>();
        InventoryItem attackerItem = attacker != null ? attacker.equippedItem.Get() : null;

        EventInstance instance = RuntimeManager.CreateInstance(weaponImpactSoundEvent);
        if (attackerItem != null)
        {
            instance.setParameterByNameWithLabel("ItemType", attackerItem.itemType.ToString());
        }
        instance.setParameterByNameWithLabel("CharacterType", GetCharacterTypeLabel());
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(gridPlaceable.transform.position));
        instance.start();
        instance.release();
    }

    private void PlayGripReducedSound(WeaponItem weaponItem)
    {
        if (Trainer.IsTraining) return;
        if (gripReducedSoundEvent.IsNull) return;

        EventInstance instance = RuntimeManager.CreateInstance(gripReducedSoundEvent);
        instance.setParameterByName("GripAmount", weaponItem.currentGrip);
        instance.setParameterByNameWithLabel("CharacterType", GetCharacterTypeLabel());
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(gridPlaceable.transform.position));
        instance.start();
        instance.release();
    }

    private void PlayDeathSound()
    {
        if (Trainer.IsTraining) return;
        if (entityDeathSoundEvent.IsNull) return;

        EventInstance instance = RuntimeManager.CreateInstance(entityDeathSoundEvent);
        instance.setParameterByNameWithLabel("CharacterType", GetCharacterTypeLabel());
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(gridPlaceable.transform.position));
        instance.start();
        instance.release();
    }
}
