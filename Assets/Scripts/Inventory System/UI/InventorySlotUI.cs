using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

using System;

/// <summary>
/// Manages the visual representation of an inventory slot in the UI.
/// Displays item icon, amount text, and handles slot assignment/clearing.
/// Usage: Attach to inventory slot UI GameObjects in the scene. Works with InventoryItemDragger for drag/drop.
/// </summary>
public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    public Action<InventorySlotUI, PointerEventData.InputButton> OnSlotClicked;

    /// <summary>Text display showing the item quantity in this slot.</summary>
    public TextMeshProUGUI amountText;

    /// <summary>Image component displaying the item's icon.</summary>
    public Image slotImage;

    /// <summary>The inventory slot data this UI represents.</summary>
    public InventorySlot slot;

    /// <summary>
    /// Updates the visual display based on the current slot's item and amount.
    /// Shows icon and quantity if item exists, otherwise hides display.
    /// </summary>
    private void UpdateUI(InventoryItem item, int amount)
    {
        if (item != null)
        {
            OnItemAdded(item, amount);
        }
        else
        {
            OnItemRemoved(item, amount);
        }
    }

    /// <summary>
    /// Updates the slot image and text when an item is added.
    /// </summary>
    private void OnItemAdded(InventoryItem item, int amount)
    {
        slotImage.sprite = item.itemIcon;
        amountText.text = amount > 1 ? amount.ToString() : "";
        slotImage.enabled = true;
    }

    /// <summary>
    /// Clears the slot image and text when an item is removed.
    /// </summary>
    private void OnItemRemoved(InventoryItem item, int amount)
    {
        slotImage.sprite = null;
        amountText.text = "";
        slotImage.enabled = false;
    }


    /// <summary>
    /// Transfers items from this slot to another slot UI.
    /// Updates both UIs after successful transfer.
    /// </summary>
    /// <param name="newSlot">The target slot UI to transfer items to.</param>
    /// <returns>True if transfer succeeded, false if slot is null or transfer failed.</returns>
    public bool TransferTo(InventorySlotUI newSlot)
    {
        if (newSlot == null || slot == null)
        {
            return false;
        }
        return slot.TransferTo(newSlot.slot);
    }

    /// <summary>
    /// Assigns a new inventory slot to this UI element and refreshes the display.
    /// </summary>
    /// <param name="newSlot">The inventory slot to assign.</param>
    public void Assign(InventorySlot newSlot)
    {
        slot = newSlot;
        UpdateUI(slot.item, slot.amount);
        slot.OnItemAdded.AddListener(OnItemAdded);
        slot.OnItemRemoved.AddListener(OnItemRemoved);
    }

    /// <summary>
    /// Clears the slot assignment and updates the display to empty.
    /// </summary>
    public void Clear()
    {
        if (slot == null) return;
        slot.OnItemAdded.RemoveListener(OnItemAdded);
        slot.OnItemRemoved.RemoveListener(OnItemRemoved);
        slot = null;
        OnItemRemoved(null, 0);
    }

    public void DiscardItem()
    {
        if (slot != null)
        {
            slot.Discard();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnSlotClicked?.Invoke(this, eventData.button);
    }
}