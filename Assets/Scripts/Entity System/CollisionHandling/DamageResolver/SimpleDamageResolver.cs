//using System;
//using UnityEngine;
//using Unity.MLAgents;
//[Serializable]
//public class SimpleDamageResolver : DamageResolver
//{
//    public int power { get; private set; }
//    public Action OnDamageTaken;
//    public Action OnDamageDealt;
//    //public Action<Entity> OnDespawn;
//    //Entity thisEntity;

//    public void Initialize(CollisionResolver collisionResolver, PickupHandler pickupHandler)
//    {
//        OnDamageTaken = null;
//        OnDamageDealt = null;
//        collisionResolver.OnCollision += OnCollision;
//        pickupHandler.OnPickupCollected += IncreasePower;
//        power = 1;
//    }
//    public void IncreasePower(Pickup pickup)
//    {
//        power++;
//    }

//    public bool IsGreater(SimpleDamageResolver other)
//    {
//        return power > other.power;
//    }
//    public void OnCollision(GameObject other)
//    {
//        if (other.TryGetComponent(out Entity entity))
//        {
//            SimpleDamageResolver otherDamageResolver = entity.damageResolver as SimpleDamageResolver;
//            if (IsGreater(otherDamageResolver))
//            {
//                otherDamageResolver.OnDamageTaken?.Invoke();
//                OnDamageDealt?.Invoke();
//            }
//        }
//    }
//}