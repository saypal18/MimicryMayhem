//using UnityEngine.EventSystems;
//using UnityEngine;
//public class PlayerInventoryUIInitializer
//{
//    InventoryUI inventoryUI;
//    public PlayerInventoryUIInitializer(PlayerStorageSettings settings)
//    {
//        inventoryUI = PoolingEntity.Spawn(settings.inventoryUIPrefab, settings.playerStorageParent);
//        //if (inventoryUI.closeButton != null)
//        //{
//        //    inventoryUI.closeButton.onClick.RemoveAllListeners();
//        //    inventoryUI.closeButton.onClick.AddListener(() => Close(inventoryUI));
//        //}
//        //else
//        //{
//        //    Debug.LogError("InventoryUI close button not assigned in the Inspector");
//        //}

//        //if (inventoryUI.eventTrigger != null)
//        //{
//        //    inventoryUI.eventTrigger.triggers.Clear();

//        //    EventTrigger.Entry enterEntry = new EventTrigger.Entry();
//        //    enterEntry.eventID = EventTriggerType.PointerEnter;
//        //    enterEntry.callback.AddListener((data) => { uiManager.OnMouseEnterCanvasElement(); });
//        //    inventoryUI.eventTrigger.triggers.Add(enterEntry);

//        //    EventTrigger.Entry exitEntry = new EventTrigger.Entry();
//        //    exitEntry.eventID = EventTriggerType.PointerExit;
//        //    exitEntry.callback.AddListener((data) => { uiManager.OnMouseExitCanvasElement(); });
//        //    inventoryUI.eventTrigger.triggers.Add(exitEntry);
//        //}
//        //else
//        //{
//        //    Debug.LogError("InventoryUI eventTrigger not assigned in the Inspector");
//        //}

//        //inventoryUI.Assign(unit);
//        //inventoryUI.Assign(unit);
//        //inventoryUI.inventoryDragger.canvas = inventoryCanvas;
//        //openInventories.Add(inventoryUI);
//    }

//    public void Assign(Entity entity)
//    {
//        inventoryUI.Assign(entity);

//    }
//}