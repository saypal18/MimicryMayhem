//using NUnit.Framework;
//using UnityEngine;
//using System.Collections.Generic;
//using System;

///// <summary>
///// Manages the inventory for a character unit in the game.
///// Handles initialization of player inventory and test item setup.
///// Usage: Attach to character GameObjects that need inventory management.
///// </summary>
//[RequireComponent(typeof(Entity))]
//public class CharacterInventoryManager : MonoBehaviour
//{
//    /// <summary>Reference to the player's inventory containing equipment slots and storage.</summary>
//    public Inventory playerInventory;
//    [SerializeField] private Entity self;

//    /// <summary>
//    /// Initializes the component with a specific number of storage slots.
//    /// </summary>
//    /// <param name="storageSlots">The amount of storage slots</param>
//    public void Initialize(int storageSlots)
//    {
//        playerInventory = new Inventory(storageSlots, 1);
//        playerInventory.OnItemAdded.AddListener(OnItemAdded);
//        playerInventory.OnItemRemoved.AddListener(OnItemRemoved);
//    }
//    private void OnItemAdded(InventoryItem item, int amount, int index, Type holderType)
//    {
//        if (holderType == typeof(GearSlots))
//        {
//            ApplyEffectOnEquip(self, item);
//        }
//    }

//    private void OnItemRemoved(InventoryItem item, int amount, int index, Type holderType)
//    {
//        if (holderType == typeof(GearSlots))
//        {
//            RemoveEffectOnUnequip(self, item);
//        }
//    }

//    /// <summary>
//    /// Applies the effect of an item to an entity when equipped.
//    /// </summary>
//    private void ApplyEffectOnEquip(Entity entity, InventoryItem item)
//    {
//        if (item is EquippableInventoryItem equippableItem)
//        {
//            equippableItem.OnEquip(entity);
//        }
//    }

//    /// <summary>
//    /// Removes the effect of an item from an entity when unequipped.
//    /// </summary>
//    private void RemoveEffectOnUnequip(Entity entity, InventoryItem item)
//    {
//        if (item is EquippableInventoryItem equippableItem)
//        {
//            equippableItem.OnUnequip(entity);
//        }
//    }

//    /// <summary>
//    /// Adds an item directly to the inventory.
//    /// Intended for automation or debugging purposes where UI interaction is bypassed.
//    /// </summary>
//    /// <param name="item">The item to add.</param>
//    public void AddItem(InventoryItem item)
//    {
//        playerInventory.AddItem(item);
//    }

//    /////////////////////REMOVE AFTER TESTING/////////////////////

//    [Header("Test Configuration")]
//    [SerializeField] private List<InventoryItem> items;

//    /// <summary>
//    /// Initializes the player inventory with default capacity and adds test items.
//    /// </summary>
//    public void Initialize()
//    {
//        //playerInventory = new Inventory(6, 1);
//        Initialize(6);
//        foreach (InventoryItem item in items)
//        {
//            playerInventory.AddItem(item);
//        }
//    }
//    /////////////////////REMOVE AFTER TESTING/////////////////////
//}