using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ShieldHintQuest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private Player playerScript;

    [Header("Settings")]
    [SerializeField] private string hintText = "Here you can find [h]SHIELD[/h]. Dashing into an opponent with shield will [h]STUN[/h] them.";
    [SerializeField] private string categoryKey = "50_shield_hint";
    [SerializeField] private int grid2Index = 1;

    private Entity playerEntity;
    private bool hintShown = false;
    private Grid grid2;
    private bool hasShield = false;

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
            CheckInitialInventory();
        }

        var environments = ReflectionUtils.GetFieldValue<List<GameInitializer>>(playerScript, "environments");
        if (environments != null && environments.Count > grid2Index)
        {
            grid2 = environments[grid2Index].grid;
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
        if (HasShield())
        {
            hasShield = true;
            RemoveHint();
            this.enabled = false;
        }
    }

    private bool HasShield()
    {
        if (playerEntity == null || playerEntity.inventory == null) return false;
        return playerEntity.inventory.GetSlots().Any(s => s.item != null && s.item.itemType == ItemType.Shield);
    }

    private void HandleItemAdded(InventoryItem item, int amount, int index)
    {
        if (item.itemType == ItemType.Shield)
        {
            hasShield = true;
            RemoveHint();
            this.enabled = false;
        }
    }

    private void Update()
    {
        if (playerEntity == null || hasShield) return;

        if (!hintShown && playerEntity.CurrentGrid == grid2)
        {
            ShowHint();
        }
    }

    private void ShowHint()
    {
        if (questManager != null)
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
