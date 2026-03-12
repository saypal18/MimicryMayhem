using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
[Serializable]
public class SortedInventory : InventorySlotHolder
{

    public int highestTier = 0;

    //public SortedInventory(int slotCount, SlotType slotType)
    //{
    //    this.slotCount = slotCount;
    //    this.slotType = slotType;

    //    for (int i = 0; i < slotCount; i++)
    //    {
    //        AddSlot(new InventorySlot(slotType, 1));
    //    }
    //}
    public void Initialize()
    {
        for (int i = 0; i < slotCount; i++)
        {
            AddSlot(new InventorySlot(slotType, 1));
        }

        // Update highest tier whenever an item is added or removed
        OnItemAdded.AddListener((item, amount, index) => UpdateHighestTier());
        OnItemRemoved.AddListener((item, amount, index) => UpdateHighestTier());
    }

    public void UpdateHighestTier()
    {
        highestTier = 0;
        foreach (var slot in slots)
        {
            if (slot.item != null && slot.item is WeaponItem weaponItem)
            {
                if (weaponItem.tier > highestTier)
                {
                    highestTier = weaponItem.tier;
                }
            }
        }
    }

    public override bool AddItem(InventoryItem item, int amount)
    {
        if (!(item is WeaponItem weaponItem))
        {
            return false;
        }

        List<WeaponItem> currentItems = new List<WeaponItem>();
        foreach (var slot in slots)
        {
            if (slot.item != null && slot.item is WeaponItem w)
            {
                currentItems.Add(w);
            }
        }

        List<WeaponItem> combinedItems = new List<WeaponItem>(currentItems);

        int initialCountOfNewItem = currentItems.Count(x => x == weaponItem);

        for (int i = 0; i < amount; i++)
        {
            combinedItems.Add(weaponItem);
        }

        List<WeaponItem> sortedItems = new List<WeaponItem>();
        var groupedByTier = combinedItems.GroupBy(x => x.tier).OrderByDescending(g => g.Key);

        foreach (var tierGroup in groupedByTier)
        {
            var swords = new Queue<WeaponItem>(tierGroup.Where(x => x.itemType == ItemType.Sword));
            var bows = new Queue<WeaponItem>(tierGroup.Where(x => x.itemType == ItemType.Bow));
            var shields = new Queue<WeaponItem>(tierGroup.Where(x => x.itemType == ItemType.Shield));

            while (swords.Count > 0 || bows.Count > 0 || shields.Count > 0)
            {
                if (swords.Count > 0) sortedItems.Add(swords.Dequeue());
                if (bows.Count > 0) sortedItems.Add(bows.Dequeue());
                if (shields.Count > 0) sortedItems.Add(shields.Dequeue());
            }
        }

        // We calculate max slots that can be filled.
        if (sortedItems.Count > slotCount)
        {
            sortedItems = sortedItems.Take(slotCount).ToList();
        }

        // Verify if the newly added item survived truncation.
        int finalCountOfNewItem = sortedItems.Count(x => x == weaponItem);
        int amountAdded = finalCountOfNewItem - initialCountOfNewItem;

        // If the new item was completely truncated, return false and skip updates to slots.
        if (amountAdded <= 0)
        {
            return false;
        }

        // Clear existing slots safely before adding new logic
        foreach (var slot in slots)
        {
            slot.Remove();
        }

        int currentSlotIdx = 0;
        foreach (var wItem in sortedItems)
        {
            if (currentSlotIdx < slotCount)
            {
                slots[currentSlotIdx].Add(wItem, 1);
                currentSlotIdx++;
            }
        }

        UpdateHighestTier();

        return true;
    }
}