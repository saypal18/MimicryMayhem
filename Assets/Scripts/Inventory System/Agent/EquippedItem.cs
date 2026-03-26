using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class EquippedItem : MonoBehaviour, IScrollHandler
{
    private int index;
    private InventorySlotHolder inventory;
    public Action<int> OnScroll; // invoked when the index changes due to scrolling

    public void Initialize(InventorySlotHolder inventory)
    {
        this.inventory = inventory;
        index = 0;
        OnScroll = null;
    }

    public InventoryItem Get()
    {
        if (inventory == null) return null;
        return inventory.GetSlot(index).item;
    }

    public int GetIndex()
    {
        return index;
    }

    public void SetIndex(int newIndex)
    {
        if (inventory != null && newIndex >= 0 && newIndex < inventory.slotCount)
        {
            index = newIndex;
            OnScroll?.Invoke(index);
        }
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
