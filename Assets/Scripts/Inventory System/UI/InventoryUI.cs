using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
/// <summary>
/// Main UI controller for the inventory system.
/// Manages equipment slots (head, body, hands, etc.) and storage panel.
/// Usage: Attach to the main inventory panel GameObject. Singleton pattern ensures one instance.
/// Requires references to equipment slot UIs and storage panel to be set in inspector.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    /// <summary>Prefab used to instantiate storage slot UI elements.</summary>
    public InventorySlotUI InventorySlotUIPrefab;

    /// <summary>List of storage slot UI elements, dynamically created.</summary>
    public List<InventorySlotUI> slotUis = new List<InventorySlotUI>();

    /// <summary>Container for storage slot UI elements.</summary>
    public RectTransform storagePanel;

    /// <summary>Root GameObject for the entire inventory panel.</summary>
    //public GameObject inventoryPanel;

    /// <summary>Number of storage slots to create.</summary>
    public int storageSize;

    /// <summary>Reference to the character whose inventory is currently displayed.</summary>
    public Entity assignedCharacter;

    //public Button closeButton;
    public UIDragger inventoryDragger;

    public EventTrigger eventTrigger;
    public Image selectedItemHighlight;
    /// <summary>
    /// Assigns a unit's inventory to this UI for display.
    /// </summary>
    /// <param name="unit">The unit whose inventory should be displayed.</param>
    public void Assign(Entity unit)
    {
        if (unit == null)
        {
            Debug.LogError("InventoryUI Assign called with null unit!");
            return;
        }
        assignedCharacter = unit;
        //if (!unit.TryGetComponent(out assignedCharacter))
        //{
        //    Debug.LogError("Assigned Character has no CharacterInventoryManager attached!");
        //    return;
        //}
        UpdateInventory(assignedCharacter.inventory);
        //assignedCharacter.itemEquip.onItemSelectionChanged += UpdateSelectedItem;
        //StartCoroutine(UpdateSelectedItemNextFrame());
    }

    /// <summary>
    /// Updates all UI elements to reflect the current state of the inventory.
    /// Assigns equipment slots and storage slots to their corresponding UI elements.
    /// </summary>
    /// <param name="playerInventory">The inventory to display.</param>
    public void UpdateInventory(InventorySlotHolder playerInventory)
    {
        storageSize = playerInventory.slotCount;
        while (slotUis.Count < storageSize)
        {
            // InventorySlotUI slotUi = PoolingEntity.Spawn(InventorySlotUIPrefab, storagePanel); // Not required as we are never really destroying slotUIs
            InventorySlotUI slotUi = Instantiate(InventorySlotUIPrefab, storagePanel);
            slotUis.Add(slotUi);
        }

        for (int i = 0; i < slotUis.Count; i++)
        {
            if (i < storageSize)
            {
                slotUis[i].Assign(playerInventory.GetSlot(i));
            }
            else
            {
                slotUis[i].Clear();
            }
        }
    }

    public void AssignInventory(InventorySlotHolder inventory)
    {
        UpdateInventory(inventory);
        storageSize = inventory.slotCount;
        for (int i = 0; i < slotUis.Count; i++)
        {
            slotUis[i].Assign(inventory.GetSlot(i));
        }

    }
    public void AssignEquippedItem(EquippedItem item)
    {
        item.OnScroll += UpdateSelectedItem;
    }

    public void UpdateSelectedItem(int index)
    {
        selectedItemHighlight.transform.position = slotUis[index].transform.position;
    }

    //private System.Collections.IEnumerator UpdateSelectedItemNextFrame()
    //{
    //    yield return null;
    //    UpdateSelectedItem(0);
    //}
}