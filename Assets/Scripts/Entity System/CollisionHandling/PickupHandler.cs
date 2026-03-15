using System;
using UnityEngine;
[Serializable]

public class PickupHandler
{
    public Action<Pickup> OnPickupCollected;
    private GameObject thisObject;
    public void Initialize(CollisionResolver collisionResolver)
    {
        OnPickupCollected = null;
        collisionResolver.OnCollision += OnPickup;
        thisObject = collisionResolver.gameObject;
    }
    private void OnPickup(GameObject other)
    {
        if (other != null && other.TryGetComponent(out Pickup pickup))
        {
            pickup.Collected(thisObject);
            OnPickupCollected?.Invoke(pickup);
        }
    }

}