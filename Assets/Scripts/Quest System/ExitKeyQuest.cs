using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ExitKeyQuest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private Player playerScript;

    [Header("Settings")]
    [SerializeField] [TextArea(3, 10)] private string findKeyHintText = "Find the [h]KEY[/h] to the [h]EXIT[/h]. It is hidden with one of the enemies. [h]FINISH[/h] him to find the key.";
    [SerializeField] [TextArea(3, 10)] private string gotKeyHintText = "Good job! You got the [h]key[/h]! Find the [h]EXIT[/h] on the left.";
    [SerializeField] private string category1Key = "70_exit_key";
    [SerializeField] private string category2Key = "71_got_key";
    [SerializeField] private int grid4Index = 3;
    [SerializeField] private int minTier = 3;

    private Entity playerEntity;
    private bool findKeyHintShown = false;
    private bool gotKeyHintShown = false;
    private Grid grid4;

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

        if (playerEntity != null && playerEntity.inventory != null)
        {
            playerEntity.inventory.OnItemAdded.AddListener(HandleItemAdded);
        }

        var environments = ReflectionUtils.GetFieldValue<List<GameInitializer>>(playerScript, "environments");
        if (environments != null && environments.Count > grid4Index)
        {
            grid4 = environments[grid4Index].grid;
        }
    }

    private void CleanupListeners()
    {
        if (playerEntity != null && playerEntity.inventory != null)
        {
            playerEntity.inventory.OnItemAdded.RemoveListener(HandleItemAdded);
        }
    }

    private void HandleItemAdded(InventoryItem item, int amount, int index)
    {
        // Transition from hint 1 to hint 2
        if (findKeyHintShown && !gotKeyHintShown && !(item is WeaponItem))
        {
            RemoveFindKeyHint();
            ShowGotKeyHint();
        }
    }

    private void Update()
    {
        if (playerEntity == null) return;

        // Condition for showing the FIRST hint: in Grid 4 and have Tier 3+ weapon
        if (!findKeyHintShown && !gotKeyHintShown && playerEntity.CurrentGrid == grid4 && HasHighTierWeapon())
        {
            ShowFindKeyHint();
        }
        
        // Removal logic for SECOND hint: remove when leaving Grid 4
        if (gotKeyHintShown && playerEntity.CurrentGrid != grid4)
        {
            RemoveGotKeyHint();
            this.enabled = false;
        }
    }

    private bool HasHighTierWeapon()
    {
        if (playerEntity == null || playerEntity.inventory == null) return false;
        return playerEntity.inventory.GetSlots().Any(s => s.item != null && s.item is WeaponItem w && w.tier >= minTier);
    }

    private void ShowFindKeyHint()
    {
        if (questManager != null)
        {
            questManager.SetQuestText(category1Key, findKeyHintText);
            findKeyHintShown = true;
        }
    }

    private void RemoveFindKeyHint()
    {
        if (questManager != null && findKeyHintShown)
        {
            questManager.RemoveCategory(category1Key);
        }
    }

    private void ShowGotKeyHint()
    {
        if (questManager != null)
        {
            questManager.SetQuestText(category2Key, gotKeyHintText);
            gotKeyHintShown = true;
        }
    }

    private void RemoveGotKeyHint()
    {
        if (questManager != null && gotKeyHintShown)
        {
            questManager.RemoveCategory(category2Key);
            gotKeyHintShown = false;
        }
    }
}
