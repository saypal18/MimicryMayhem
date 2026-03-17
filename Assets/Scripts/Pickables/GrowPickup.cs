using UnityEngine;


public class GrowPickup : Pickup
{
    public override bool Collected(GameObject picker)
    {
        PoolingEntity.Despawn(gameObject);
        return true;
    }

}