using System;
using UnityEngine;
[Serializable]

public class PickupHandler
{
    public Action<Pickup> OnPickupCollected;
    [SerializeField] private GameObject thisObject;
    public void Initialize()
    {
        OnPickupCollected = null;
    }
    public void OnPickup(GameObject other)
    {
        if (other != null && other.TryGetComponent(out Pickup pickup))
        {
            if (pickup.Collected(thisObject))
            {
                OnPickupCollected?.Invoke(pickup);
            }
        }
    }

}