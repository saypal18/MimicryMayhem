using UnityEngine;
using System;
public class DamageDealer : MonoBehaviour
{
    public int tier;
    public Vector2Int direction;
    public bool applyKnockback;
    public Action OnDamageDealt;
    

    public void Initialize()
    {
        OnDamageDealt = null;
        tier = 0;
        direction = Vector2Int.zero;
        applyKnockback = false;
    }
    // update tier on damage resolver and active ability change

    public void UpdateDamage(WeaponItem weapon)
    {
        if (weapon == null)
        {
            applyKnockback = false;
            tier = 0;
            return;
        }
        tier = weapon.tier;
        if (weapon.itemType == ItemType.Sword || weapon.itemType == ItemType.Shield)
        {
            applyKnockback = true;
        }
        else
        {
            applyKnockback = false;
        }
    }

}