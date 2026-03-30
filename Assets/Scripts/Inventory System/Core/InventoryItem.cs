using UnityEngine;

/// <summary>
/// ScriptableObject representing an inventory item and its properties.
/// Usage: Create via <b>Assets &gt; Create &gt; Inventory &gt; Item</b> menu in Unity.
/// Configure item type, name, icon, description, stack size, and equipped effect in the inspector.
/// </summary>
[CreateAssetMenu(fileName = "NewInventoryItem", menuName = "Inventory/Item")]
public class InventoryItem : ScriptableObject
{
    /// <summary>The category/type of this item (e.g., Helmet, Weapon, Consumable, etc.).</summary>
    public ItemType itemType;

    /// <summary>The display name of the item.</summary>
    public string itemName;

    /// <summary>The sprite icon representing this item in the UI.</summary>
    public Sprite itemIcon;

    /// <summary>The sprite icon specifically used for the inventory slot display.</summary>
    public Sprite inventoryIcon;

    /// <summary>Descriptive text explaining the item's properties or effects.</summary>
    public string description;

    /// <summary>Maximum number of this item that can stack in a single slot.</summary>
    public int maxStackSize = 1;

    /// <summary>Data defining the effect this item applies when equipped (optional).</summary>

}