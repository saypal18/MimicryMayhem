using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class EquippedItem : IScrollHandler
{
    private int index;
    private InventorySlotHolder inventory;
    public Action<int> OnScroll;
    public void Initialize(InventorySlotHolder inventory)
    {
        this.inventory = inventory; 
        index = 0;
        OnScroll = null;
    }
    public InventoryItem Get()
    {
        return inventory.GetSlot(index).item;
    }
    public int GetIndex()
    {
        return index;
    }

    public void HandleScroll(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            float scrollValue = context.ReadValue<float>();
            if (scrollValue > 0)
            {
                index = (index + 1) % inventory.slotCount;
            }
            else if (scrollValue < 0)
            {
                index = (index - 1 + inventory.slotCount) % inventory.slotCount;
            }
            OnScroll?.Invoke(index);
        }
    }
}