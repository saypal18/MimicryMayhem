using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Represents a single inventory slot that can contain a stack of items.
/// Handles item storage, stacking, compatibility checking, effect application, and transfers.
/// Usage: Created programmatically by <see cref="Inventory"/> and <see cref="StorageInventory"/> classes.
/// </summary>
[System.Serializable]
public class InventorySlot
{
    /// <summary>The type of slot, determining what items can be placed here (e.g., Helmet, Armor, MainHand, etc.).</summary>
    public SlotType slotType;

    /// <summary>The item currently stored in this slot, or null if empty.</summary>
    public InventoryItem item;

    /// <summary>The quantity of items in this slot.</summary>
    public int amount;

    /// <summary>Maximum number of items that can be stacked in this slot.</summary>
    public int maxStackSize;

    /// <summary>The entity that owns this slot (for effect application).</summary>


    public UnityEvent<InventoryItem, int> OnItemAdded = new();
    public UnityEvent<InventoryItem, int> OnItemRemoved = new();

    /// <summary>
    /// Creates a general-purpose inventory slot with the default slot type (General).
    /// </summary>
    /// <param name="maxStackSize">Maximum items that can stack in this slot.</param>
    public InventorySlot(int maxStackSize) : this(SlotType.General, maxStackSize) { }
    /// <summary>
    /// Creates a specialized inventory slot for specific equipment types.
    /// </summary>
    /// <param name="slotType">The type of items this slot accepts (Helmet, Armor, etc.).</param>
    /// <param name="maxStackSize">Maximum items that can stack in this slot.</param>


    /// <summary>
    /// Creates an inventory slot with full configuration.
    /// </summary>
    /// <param name="slotType">The type of items this slot accepts.</param>
    /// <param name="maxStackSize">Maximum items that can stack in this slot.</param>
    /// <param name="ownerEntity">The entity that owns this slot.</param>
    public InventorySlot(SlotType slotType, int maxStackSize)
    {

        this.maxStackSize = maxStackSize;
        item = null;
        amount = 0;
        this.slotType = slotType;

    }

    /// <summary>
    /// Adds a single item to the slot.
    /// </summary>
    /// <param name="newItem">The item to add.</param>
    /// <returns>True if successfully added, false if incompatible or full.</returns>
    public bool Add(InventoryItem newItem)
    {
        return Add(newItem, 1);
    }

    /// <summary>
    /// Attempts to add items to this slot. Checks compatibility and stacking limits.
    /// Will only add if: item matches existing item (or slot is empty), slot type is compatible,
    /// and there's enough space within stack limits and the item's own max stack size.
    /// </summary>
    /// <param name="newItem">The item to add.</param>
    /// <param name="amountToAdd">Number of items to add.</param>
    /// <returns>True if items were successfully added, false otherwise.</returns>
    public bool Add(InventoryItem newItem, int amountToAdd)
    {
        bool added = false;
        if (!SlotCompatibility.IsItemCompatibleWithSlot(newItem.itemType, slotType))
        {
            Debug.Log("Unable to Add Item. Item type is not compatible with slot type.");
        }
        else if (item != null)
        {
            int newAmount = amount + amountToAdd;
            if (item == newItem && newAmount <= maxStackSize && newAmount <= newItem.maxStackSize)
            {
                amount = newAmount;
                added = true;
            }
            else
            {
                Debug.Log("Unable to Add Item. Slot already contains a different item or is full.");
            }
        }
        else if (amountToAdd <= maxStackSize && amountToAdd <= newItem.maxStackSize)
        {
            item = newItem;
            amount = amountToAdd;
            added = true;
        }

        if (added)
        {
            // OnItemAdded(item, amount);
            OnItemAdded?.Invoke(item, amount);
        }
        return added;
    }



    /// <summary>
    /// Clears the slot, removing all items and any applied effects.
    /// </summary>
    /// <returns>Always returns true.</returns>
    public bool Remove()
    {

        InventoryItem removedItem = item;
        int removedAmount = amount;
        item = null;
        amount = 0;
        // OnItemRemoved(removedItem, removedAmount);
        OnItemRemoved?.Invoke(removedItem, removedAmount);
        return true;
    }

    public bool Discard()
    {
        if (item == null) return false;
        if (InventoryUtils.IsDiscardable(item.itemType))
        {
            Remove();
            return true;
        }
        return false;
    }



    /// <summary>
    /// Transfers all items from this slot to another slot.
    /// Checks compatibility and stacking before transferring.
    /// </summary>
    /// <param name="targetSlot">The destination slot.</param>
    /// <returns>True if transfer successful, false if incompatible or target is full.</returns>
    public bool TransferTo(InventorySlot targetSlot)
    {
        if (item == null || targetSlot == null)
        {
            return false;
        }
        if (!SlotCompatibility.IsItemCompatibleWithSlot(item.itemType, targetSlot.slotType))
        {
            Debug.Log("Cannot transfer item: incompatible slot type.");
            return false;
        }
        if (targetSlot.Add(item, amount))
        {
            Remove();
            return true;
        }
        return false;
    }
}
