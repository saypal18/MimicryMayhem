using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class HighTierAreaWarningQuest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private Player playerScript;

    [Header("Settings")]
    [SerializeField] private string warningText = "You aren't equipped well to handle this area. You might get [h]OBLITERATED[/h]";
    [SerializeField] private string categoryKey = "50_area_warning";
    [SerializeField] private int requiredTier = 3;

    [Header("Grid Info")]
    [SerializeField] private int grid1Index = 0;
    [SerializeField] private int grid2Index = 1;
    [SerializeField] private int grid4Index = 3;

    private Entity playerEntity;
    private bool hintShown = false;
    private Grid grid1;
    private Grid grid2;
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

        // Find Grid references (requires grid mapping to be correct in environments list)
        var environments = ReflectionUtils.GetFieldValue<List<GameInitializer>>(playerScript, "environments");
        if (environments != null)
        {
            if (environments.Count > grid1Index) grid1 = environments[grid1Index].grid;
            if (environments.Count > grid2Index) grid2 = environments[grid2Index].grid;
            if (environments.Count > grid4Index) grid4 = environments[grid4Index].grid;
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
        if (item is WeaponItem weapon && weapon.tier >= requiredTier)
        {
            RemoveHint();
        }
    }

    private void Update()
    {
        if (playerEntity == null) return;

        bool inGrid4 = playerEntity.CurrentGrid == grid4;
        bool inSafeGrid = playerEntity.CurrentGrid == grid1 || playerEntity.CurrentGrid == grid2;
        bool hasHighTier = HasHighTierWeapon();

        if (inGrid4 && !hasHighTier && !hintShown)
        {
            ShowHint();
        }
        else if (hintShown)
        {
            if (hasHighTier || inSafeGrid)
            {
                RemoveHint();
            }
        }
    }

    private bool HasHighTierWeapon()
    {
        if (playerEntity == null || playerEntity.inventory == null) return false;
        return playerEntity.inventory.GetSlots().Any(s => s.item != null && s.item is WeaponItem w && w.tier >= requiredTier);
    }

    private void ShowHint()
    {
        if (questManager != null)
        {
            questManager.SetQuestText(categoryKey, warningText);
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
