using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CombatSystemHintQuest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private Player playerScript;

    [Header("Settings")]
    [SerializeField] [TextArea(3, 10)] private string hintText = "Weapons have [h]TIER[/h] and [h]GRIP[/h]. Hits from equal or lower tiers drain [h]1[/h] grip. If [h]ATTACKER TIER > DEFENDER GRIP[/h], the weapon is stolen and they are [h]DEFEATED[/h].";
    [SerializeField] private string categoryKey = "80_combat_tutorial";

    private Entity playerEntity;
    private bool hintShown = false;
    private bool initializedKeys = false;
    private HashSet<string> keysAtShowTime = new HashSet<string>();

    private void OnEnable()
    {
        if (playerScript != null)
        {
            playerScript.OnPlayerSpawned += Initialize;
        }
        if (questManager != null)
        {
            questManager.OnQuestChanged.AddListener(HandleQuestChanged);
        }
    }

    private void OnDisable()
    {
        if (playerScript != null)
        {
            playerScript.OnPlayerSpawned -= Initialize;
        }
        if (questManager != null)
        {
            questManager.OnQuestChanged.RemoveListener(HandleQuestChanged);
        }
        CleanupListeners();
    }

    private void Initialize(Entity entity)
    {
        CleanupListeners();
        playerEntity = entity;

        if (playerEntity != null && playerEntity.inventory != null)
        {
            playerEntity.inventory.OnItemAdded.AddListener(HandleItemAdded);
            CheckInitialInventory();
        }
    }

    private void CleanupListeners()
    {
        if (playerEntity != null && playerEntity.inventory != null)
        {
            playerEntity.inventory.OnItemAdded.RemoveListener(HandleItemAdded);
        }
    }

    private void CheckInitialInventory()
    {
        if (!hintShown && HasShield())
        {
            ShowHint();
        }
    }

    private bool HasShield()
    {
        if (playerEntity == null || playerEntity.inventory == null) return false;
        return playerEntity.inventory.GetSlots().Any(s => s.item != null && s.item.itemType == ItemType.Shield);
    }

    private void HandleItemAdded(InventoryItem item, int amount, int index)
    {
        if (item.itemType == ItemType.Shield && !hintShown)
        {
            ShowHint();
        }
    }

    private void ShowHint()
    {
        if (questManager != null)
        {
            hintShown = true;
            initializedKeys = false; // Will be set on the next QuestChanged callback
            questManager.SetQuestText(categoryKey, hintText);
        }
    }

    private void HandleQuestChanged(Dictionary<string, string> questData)
    {
        if (!hintShown) return;

        if (!initializedKeys)
        {
            keysAtShowTime.Clear();
            foreach (var key in questData.Keys)
            {
                if (key != categoryKey) keysAtShowTime.Add(key);
            }
            initializedKeys = true;
            return;
        }

        // Detect if ANY key is present that wasn't there before
        bool newHintDetected = false;
        foreach (var key in questData.Keys)
        {
            if (key != categoryKey && !keysAtShowTime.Contains(key))
            {
                newHintDetected = true;
                break;
            }
        }

        if (newHintDetected)
        {
            RemoveHint();
            this.enabled = false;
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
