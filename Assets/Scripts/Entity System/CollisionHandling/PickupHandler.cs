using System;
using UnityEngine;
[Serializable]

public class PickupHandler
{
    public Action<Pickup> OnPickupCollected;
    public void Initialize(CollisionResolver collisionResolver)
    {
        OnPickupCollected = null;
        collisionResolver.OnCollision += OnPickup;
    }
    private void OnPickup(GameObject other)
    {
        if (other.TryGetComponent(out Pickup pickup))
        {
            pickup.Collected(other);
            OnPickupCollected?.Invoke(pickup);
        }
    }

}