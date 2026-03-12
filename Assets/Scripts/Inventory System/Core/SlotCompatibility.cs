/// <summary>
/// Static utility class for determining item-slot compatibility.
/// Uses a compatibility matrix to validate whether an item type can be placed in a specific slot type.
/// Usage: Call IsItemCompatibleWithSlot() to check compatibility before adding items to slots.
/// </summary>
public static class SlotCompatibility
{
    /// <summary>
    /// 2D compatibility matrix mapping item types to slot types.
    /// Rows: ItemType (Sword, Bow, Shield)
    /// Columns: SlotType (General, Sword, Bow, Shield)
    /// </summary>
    private static readonly bool[,] compatibilityMatrix = new bool[,]
    {
        // General, Sword, Bow, Shield <- SlotType
        { true,    true,  false, false }, // Sword <- ItemType
        { true,    false, true,  false }, // Bow
        { true,    false, false, true  }  // Shield
    };

    /// <summary>
    /// Checks if an item type can be placed in a specific slot type.
    /// </summary>
    /// <param name="itemType">The type of item to check.</param>
    /// <param name="slotType">The type of slot to check against.</param>
    /// <returns>True if the item can be placed in the slot, false otherwise.</returns>
    public static bool IsItemCompatibleWithSlot(ItemType itemType, SlotType slotType)
    {
        return compatibilityMatrix[(int)itemType, (int)slotType];
    }
}