using UnityEngine;
using System;
using System.Collections.Generic;
public class DamageDealer : MonoBehaviour
{
    public int tier;
    public Vector2Int direction;
    public bool applyKnockback;
    public Vector2Int attackStartPosition;
    public Action<Entity> OnDamageDealt;
    public Action<Entity> OnKillDealt;
    public Entity entity;

    private HashSet<Entity> hitEntities = new();

    public void Initialize()
    {
        OnDamageDealt = null;
        OnKillDealt = null;
        tier = 0;
        direction = Vector2Int.zero;
        attackStartPosition = Vector2Int.zero;
        applyKnockback = false;
        hitEntities.Clear();
    }

    public bool TryRegisterHit(Entity target)
    {
        //Debug hit entities
        if (target == null) return false;
        if (hitEntities.Contains(target)) return false;

        hitEntities.Add(target);
        return true;
    }

    public void ResetHitTargets()
    {
        hitEntities.Clear();
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