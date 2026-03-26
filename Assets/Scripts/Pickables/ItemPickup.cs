using System;
using UnityEngine;


public class ItemPickup :  Pickup
{
    [SerializeField] private InventoryItem inventoryItem;
    public override bool Collected(GameObject picker)
    {
        if (dropper != null && picker == dropper) return false;
        if(picker.TryGetComponent(out Entity entity))
        {
            if (entity.inventory.AddItem(inventoryItem))
            {
                PoolingEntity.Despawn(gameObject);
                return true;
            }
        }
        return false;
    }

}