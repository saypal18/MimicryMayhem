using System.Collections.Generic;
public enum ItemType
{
    Sword,
    Bow,
    Shield
}


// in futre slot type will be like Ranged Unit Main Hand, Melee unit Offhand so that we can restrict equipping items based on unit type
// or we can find some way to bring in entity type into inventory system
public enum SlotType
{
    General,
    Sword,
    Bow,
    Shield

}

public class InventoryUtils
{
    private static List<ItemType> discardableItems = new List<ItemType>()
    {
    };
    public static bool IsDiscardable(ItemType type)
    {
        return discardableItems.Contains(type);
    }
}