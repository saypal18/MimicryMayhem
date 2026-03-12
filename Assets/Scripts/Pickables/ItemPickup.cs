using System;
using UnityEngine;


public class ItemPickup :  Pickup
{
    [SerializeField] private InventoryItem inventoryItem;
    public override void Collected(GameObject picker)
    {
        if(picker.TryGetComponent(out Entity entity))
        {
            entity.inventory.AddItem(inventoryItem);
        }
        PoolingEntity.Despawn(gameObject);
    }

}