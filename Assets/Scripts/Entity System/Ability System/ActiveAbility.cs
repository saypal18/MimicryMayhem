using UnityEngine;
using System;
using FMODUnity;
using FMOD.Studio;
[Serializable]
public class ActiveAbility
{
    [SerializeField] private MeleeAttack meleeAttack;
    [SerializeField] private RangeAttack rangeAttack;
    [SerializeField] private DashAttack dashAttack;
    private EquippedItem equippedItem;
    public IAbility ability = null;
    private DamageDealer damageDealer;

    [Header("Audio")]
    [SerializeField] private EventReference weaponAttackSoundEvent;

    public bool IsDashing => dashAttack != null && dashAttack.IsDashing;
    public bool IsMeleeAttacking => meleeAttack != null && meleeAttack.IsAttacking;

    public void Initialize(Grid grid, DamageDealer meleeDamageDealer, EquippedItem equippedItem, InventorySlotHolder inventory, DamageDealer damageDealer, EntityMovementFactory movementFactory, GridPlaceable gridPlaceable, MoveInfo moveInfo, Animator animator)
    {
        this.equippedItem = equippedItem;
        this.damageDealer = damageDealer;
        meleeAttack.Initialize(grid, meleeDamageDealer);
        meleeAttack.SetAnimator(animator);
        rangeAttack.Initialize(grid, damageDealer);
        rangeAttack.SetAnimator(animator);
        dashAttack.Initialize(damageDealer, movementFactory, gridPlaceable, moveInfo);
        dashAttack.SetAnimator(animator);
        //equippedItem.OnScroll += UpdateActiveAbility;
        //inventory.OnItemRemoved.AddListener((item, amount, index) => UpdateActiveAbility(-1));
        //inventory.OnItemAdded.AddListener((item, amount, index) => UpdateActiveAbility(-1));
        //UpdateActiveAbility(0);
    }

    public void UpdateGrid(Grid newGrid)
    {
        meleeAttack.Initialize(newGrid, damageDealer);
        rangeAttack.Initialize(newGrid, damageDealer);
    }

    public void UpdateActiveAbility()
    {
        InventoryItem item = equippedItem.Get();
        if (item == null || !(item is WeaponItem))
        {
            ability = null;
            damageDealer.UpdateDamage(null);
            return;
        }

        WeaponItem weapon = (WeaponItem)item;
        switch (item.itemType)
        {
            case (ItemType.Sword):
                ability = meleeAttack;
                break;
            case (ItemType.Bow):
                ability = rangeAttack;
                break;
            case (ItemType.Shield):
                ability = dashAttack;
                break;
            default:
                ability = null;
                break;
        }

        if (ability != null)
        {
            ability.Range = weapon.range;
        }

        damageDealer.UpdateDamage(weapon);
    }

    public void PlayAttackSound(Vector3 position, Entity entity)
    {
        if (Trainer.IsTraining) return;
        if (weaponAttackSoundEvent.IsNull || equippedItem == null) return;

        InventoryItem item = equippedItem.Get();
        if (item == null) return;

        string characterType = entity != null ? (entity.IsPlayer ? "Player" : entity.IsBoss ? "Boss" : "Enemy") : "Enemy";

        if (!weaponAttackSoundEvent.IsNull)
        {
            EventInstance instance = RuntimeManager.CreateInstance(weaponAttackSoundEvent);
            instance.setParameterByNameWithLabel("ItemType", item.itemType.ToString());
            instance.setParameterByNameWithLabel("CharacterType", characterType);
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
            instance.start();
            instance.release();
        }

    }
}
