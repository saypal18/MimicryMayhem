using UnityEngine;
using System.Linq;

public class InventoryFullHintQuest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private Player playerScript;

    [Header("Settings")]
    [SerializeField] private string hintText = "[h]RIGHT CLICK[/h] an item on the inventory to drop it. Your can at max pick up [h]{0}[/h] weapons.";
    [SerializeField] private string categoryKey = "40_inventory_full_hint";

    private Entity playerEntity;
    private bool hintShown = false;
    private bool firstDropDone = false;

    private void OnEnable()
    {
        if (playerScript != null)
        {
            playerScript.OnPlayerSpawned += Initialize;
        }
    }

    private void OnDisable()
    {
        if (playerScript != null)
        {
            playerScript.OnPlayerSpawned -= Initialize;
        }
        CleanupListeners();
    }

    private void Initialize(Entity entity)
    {
        CleanupListeners();
        playerEntity = entity;

        if (playerEntity != null && playerEntity.pickupHandler != null)
        {
            playerEntity.pickupHandler.OnPickupFailed += HandlePickupFailed;
        }

        if (playerEntity != null && playerEntity.inventory != null)
        {
            playerEntity.inventory.OnItemDropped += HandleItemDropped;
        }
    }

    private void CleanupListeners()
    {
        if (playerEntity != null)
        {
            if (playerEntity.pickupHandler != null)
                playerEntity.pickupHandler.OnPickupFailed -= HandlePickupFailed;

            if (playerEntity.inventory != null)
                playerEntity.inventory.OnItemDropped -= HandleItemDropped;
        }
    }

    private void HandlePickupFailed(Pickup pickup)
    {
        if (pickup is WeaponPickup && IsInventoryFull() && !firstDropDone)
        {
            ShowHint();
        }
    }

    private void HandleItemDropped(WeaponItem item, int index)
    {
        if (hintShown)
        {
            firstDropDone = true;
            RemoveHint();
            this.enabled = false;
        }
    }

    private bool IsInventoryFull()
    {
        if (playerEntity == null || playerEntity.inventory == null) return false;
        var slots = playerEntity.inventory.GetSlots();
        return slots.All(s => s.item != null);
    }

    private void ShowHint()
    {
        if (questManager != null && !hintShown)
        {
            int slotCount = playerEntity.inventory.slotCount;
            string text = string.Format(hintText, slotCount);
            questManager.SetQuestText(categoryKey, text);
            hintShown = true;
        }
    }

    private void RemoveHint()
    {
        if (questManager != null && hintShown)
        {
            questManager.RemoveCategory(categoryKey);
            hintShown = false;
        }
    }
}
