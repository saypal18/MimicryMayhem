using System;
using UnityEngine;

public class CollisionResolver : MonoBehaviour
{
    private PickupHandler pickupHandler;
    private UnifiedDamageResolver damageResolver;
    private EntityCollisionKnockback knockback;
    private AbilityController abilityController;

    private int pickupLayer;
    private int damageDealerLayer;
    private int damageResolverLayer;

    public void Initialize(PickupHandler pickupHandler, UnifiedDamageResolver damageResolver, EntityCollisionKnockback knockback, AbilityController abilityController)
    {
        this.pickupHandler = pickupHandler;
        this.damageResolver = damageResolver;
        this.knockback = knockback;
        this.abilityController = abilityController;

        pickupLayer = LayerMask.NameToLayer("Pickup");
        damageDealerLayer = LayerMask.NameToLayer("DamageDealer");
        damageResolverLayer = LayerMask.NameToLayer("DamageResolver");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent(out Root root)) return;
        if (root.GO == gameObject) return;

        int layer = collision.gameObject.layer;
        //Entity otherEntity = root.GO.GetComponent<Entity>();
        //if (otherEntity == null) return;

        if (layer == pickupLayer)
        {
            pickupHandler.OnPickup(root.GO);
            return;
        }

        Entity otherEntity = root.GO.GetComponent<Entity>();
        if (otherEntity == null) return;
        if (layer == damageDealerLayer)
        {
            if (otherEntity.damageDealer != null)
            {
                if (otherEntity.damageDealer.TryRegisterHit(GetComponentInParent<Entity>()))
                {
                    damageResolver.AcceptDamage(otherEntity.damageDealer);
                    if (otherEntity.damageDealer.applyKnockback)
                    {
                        knockback.ApplyKnockback(otherEntity, layer);
                    }
                }
            }
        }
        else if (layer == damageResolverLayer)
        {
            if (otherEntity.moveInfo.IsDashing)
            {
                if (otherEntity.damageDealer != null && otherEntity.damageDealer.TryRegisterHit(GetComponentInParent<Entity>()))
                {
                    damageResolver.AcceptDamage(otherEntity.damageDealer);
                }
            }
            knockback.ApplyKnockback(otherEntity, layer);
        }
    }
}

