using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Handles drag and drop input events for inventory items.
/// Detects pointer down/up events and communicates with InventoryItemDragged for visual feedback.
/// Usage: Attach to inventory slot UI GameObjects alongside InventorySlotUI.
/// </summary>
public class InventoryItemDragger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    /// <summary>Reference to the current inventory slot UI.</summary>
    public InventorySlotUI currentSlot;
    public Image slotImage;
    private bool beingDragged = false;
    /// <summary>
    /// Called when the pointer is pressed down on the slot. Initiates drag operation.
    /// </summary>
    /// <param name="eventData">Current event data.</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentSlot == null)
        {
            currentSlot = GetComponent<InventorySlotUI>();
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            //discard item
            currentSlot.DiscardItem();
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Handle right click logic here
            if (InventoryItemDragged.Instance.TakeFrom(currentSlot))
            {
                OnBeginDrag();
            }
        }
    }


    public void OnBeginDrag()
    {
        if (!beingDragged)
        {
            beingDragged = true;
            slotImage.color = new Color(slotImage.color.r, slotImage.color.g, slotImage.color.b, 0.5f);
        }
        else
        {
            Debug.LogError("Inventory Slot STARTING to drag twice cannot happen. Please check for bugs!");
        }

    }

    public void OnEndDrag()
    {
        if (beingDragged)
        {
            beingDragged = false;
            if (slotImage)
                slotImage.color = new Color(slotImage.color.r, slotImage.color.g, slotImage.color.b, 1f);
        }
    }

    /// <summary>
    /// Called when the pointer is released. Attempts to drop the item at the current position.
    /// </summary>
    /// <param name="eventData">Current event data.</param>
    public void OnPointerUp(PointerEventData eventData)
    {
        GameObject droppedOn = eventData.pointerCurrentRaycast.gameObject;
        InventoryItemDragged.Instance.DropAt(GetSlotAtPosition(eventData));
        OnEndDrag();
    }

    /// <summary>
    /// Finds the inventory slot UI at the pointer's position using raycasting.
    /// </summary>
    /// <param name="eventData">Current event data containing pointer position.</param>
    /// <returns>The InventorySlotUI at the position, or null if none found.</returns>
    private InventorySlotUI GetSlotAtPosition(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            InventorySlotUI slotUI = result.gameObject.GetComponent<InventorySlotUI>();
            if (slotUI != null && slotUI != InventoryItemDragged.Instance.draggedSlot)
            {
                return slotUI;
            }
        }

        return null;
    }
}