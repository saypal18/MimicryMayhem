using UnityEngine;
using System;
[Serializable]
public class ActiveAbility
{
    [SerializeField] private MeleeAttack meleeAttack;
    [SerializeField] private RangeAttack rangeAttack;
    [SerializeField] private DashAttack dashAttack;
    private EquippedItem equippedItem;
    public IAbility ability = null;
    private DamageDealer damageDealer;

    public bool IsDashing => dashAttack != null && dashAttack.IsDashing;
    public bool IsMeleeAttacking => meleeAttack != null && meleeAttack.IsAttacking;

    public void Initialize(Grid grid, DamageDealer meleeDamageDealer, EquippedItem equippedItem, InventorySlotHolder inventory, DamageDealer damageDealer, EntityMovementFactory movementFactory, GridPlaceable gridPlaceable, MoveInfo moveInfo)
    {
        this.equippedItem = equippedItem;
        this.damageDealer = damageDealer;
        meleeAttack.Initialize(grid, meleeDamageDealer);
        rangeAttack.Initialize(grid, damageDealer);
        dashAttack.Initialize(damageDealer, movementFactory, gridPlaceable, moveInfo);
        //equippedItem.OnScroll += UpdateActiveAbility;
        //inventory.OnItemRemoved.AddListener((item, amount, index) => UpdateActiveAbility(-1));
        //inventory.OnItemAdded.AddListener((item, amount, index) => UpdateActiveAbility(-1));
        //UpdateActiveAbility(0);
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
        damageDealer.UpdateDamage((WeaponItem)item);
    }
}