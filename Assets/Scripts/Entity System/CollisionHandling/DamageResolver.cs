using System;
using UnityEngine;
using Unity.MLAgents;
[Serializable]
public class DamageResolver
{
    public int power;
    public Action OnDamageTaken;
    public Action OnDamageDealt;
    //public Action<Entity> OnDespawn;
    //Entity thisEntity;

    public void Initialize(CollisionResolver collisionResolver, PickupHandler pickupHandler)
    {
        OnDamageTaken = null;
        OnDamageDealt = null;
        collisionResolver.OnCollision += OnCollision;
        pickupHandler.OnPickupCollected += IncreasePower;
        power = 1;
    }
    public void IncreasePower(Pickup pickup)
    {
        power++;
    }

    public bool IsGreater(DamageResolver other)
    {
        return power > other.power;
    }
    public void OnCollision(GameObject other)
    {
        if (other.TryGetComponent(out Entity entity))
        {
            if (IsGreater(entity.damageResolver))
            {
                entity.damageResolver.OnDamageTaken?.Invoke();
                OnDamageDealt?.Invoke();
            }
        }
    }
}