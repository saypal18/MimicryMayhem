using UnityEngine;


public class GrowPickup : Pickup
{
    public override void Collected(GameObject picker)
    {
        PoolingEntity.Despawn(gameObject);
    }

}