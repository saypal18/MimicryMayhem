using UnityEngine;
using System.Linq;

public class WeaponSwitchHintQuest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private Player playerScript;

    [Header("Settings")]
    [SerializeField] private string categoryKey = "70_weapon_switch_hint";
    [SerializeField] [TextArea] private string hintText = "you can [h]SCROLL[/h] to switch between weapons. or [h]CLICK[/h] on a weapon in the [h]INVENTORY[/h] below.";

    private Entity playerEntity;
    private bool hintShown = false;
    private bool weaponSwitched = false;

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
        
        if (playerEntity != null)
        {
            if (playerEntity.inventory != null)
            {
                playerEntity.inventory.OnItemAdded.AddListener(HandleItemAdded);
                // Check if already has 2+ weapons
                CheckWeaponCount();
            }

            if (playerEntity.equippedItem != null)
            {
                playerEntity.equippedItem.OnScroll += HandleWeaponSwitched;
            }
        }
    }

    private void CleanupListeners()
    {
        if (playerEntity != null)
        {
            if (playerEntity.inventory != null)
            {
                playerEntity.inventory.OnItemAdded.RemoveListener(HandleItemAdded);
            }
            
            if (playerEntity.equippedItem != null)
            {
                playerEntity.equippedItem.OnScroll -= HandleWeaponSwitched;
            }
        }
    }

    private void HandleItemAdded(InventoryItem item, int amount, int index)
    {
        if (!hintShown && !weaponSwitched)
        {
            CheckWeaponCount();
        }
    }

    private void CheckWeaponCount()
    {
        if (playerEntity == null || playerEntity.inventory == null) return;

        int weaponCount = playerEntity.inventory.GetSlots().Count(s => s.item != null && s.item is WeaponItem);
        if (weaponCount >= 2)
        {
            ShowHint();
        }
    }

    private void HandleWeaponSwitched(int index)
    {
        if (hintShown)
        {
            weaponSwitched = true;
            RemoveHint();
            // Disabling the quest after it's done
            this.enabled = false;
        }
    }

    private void ShowHint()
    {
        if (questManager != null && !hintShown)
        {
            questManager.SetQuestText(categoryKey, hintText);
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
