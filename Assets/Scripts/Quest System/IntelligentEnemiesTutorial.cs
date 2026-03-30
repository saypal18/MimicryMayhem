using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class IntelligentEnemiesTutorial : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private Player playerScript;

    [Header("Settings")]
    [SerializeField] [TextArea] private string hint1Text = "Enemies in this and next region are [h]INTELLIGENT[/h]. Good luck!";
    [SerializeField] [TextArea] private string hint2Text = "you can [h]FINISH[/h] off intelligent enemies. Doing so will [h]RESET the GRIP[/h] for all your weapons. But enemies don't respawn.";
    [SerializeField] private string category1Key = "60_intel_enemies";
    [SerializeField] private string category2Key = "61_finish_off";
    [SerializeField] private int grid3Index = 2;

    private Entity playerEntity;
    private bool hint1Shown = false;
    private bool hint2Shown = false;
    private Grid grid3;

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

        if (playerEntity != null && playerEntity.damageDealer != null)
        {
            playerEntity.damageDealer.OnKillDealt += HandleKillDealt;
        }

        var environments = ReflectionUtils.GetFieldValue<List<GameInitializer>>(playerScript, "environments");
        if (environments != null && environments.Count > grid3Index)
        {
            grid3 = environments[grid3Index].grid;
        }
    }

    private void CleanupListeners()
    {
        if (playerEntity != null && playerEntity.damageDealer != null)
        {
            playerEntity.damageDealer.OnKillDealt -= HandleKillDealt;
        }
    }

    private void HandleKillDealt(Entity victim)
    {
        // Only trigger hint progression and grip reset in Grid 3 and Grid 4 (intelligent regions)
        if (playerEntity == null) return;

        bool inIntelligentRegion = IsInGrid(grid3Index) || IsInGrid(3); // Grid 3 or Grid 4

        if (!inIntelligentRegion) return;

        // Progress hint 1 -> hint 2
        if (hint1Shown && !hint2Shown && IsInGrid(grid3Index))
        {
            RemoveHint1();
            ShowHint2();
        }
    }

    private bool IsInGrid(int index)
    {
        if (playerEntity == null || playerScript == null) return false;
        var environments = ReflectionUtils.GetFieldValue<List<GameInitializer>>(playerScript, "environments");
        if (environments != null && environments.Count > index)
        {
            return playerEntity.CurrentGrid == environments[index].grid;
        }
        return false;
    }

    private void Update()
    {
        if (playerEntity == null) return;

        // Show hint 1 when entering grid 3
        if (!hint1Shown && IsInGrid(grid3Index))
        {
            ShowHint1();
        }
        
        // Removal logic for hint 2: disable when leaving intelligent regions
        if (hint2Shown && !IsInGrid(grid3Index) && !IsInGrid(3))
        {
            RemoveHint2();
            this.enabled = false;
        }
    }

    private void ShowHint1()
    {
        if (questManager != null)
        {
            questManager.SetQuestText(category1Key, hint1Text);
            hint1Shown = true;
        }
    }

    private void RemoveHint1()
    {
        if (questManager != null && hint1Shown)
        {
            questManager.RemoveCategory(category1Key);
        }
    }

    private void ShowHint2()
    {
        if (questManager != null)
        {
            questManager.SetQuestText(category2Key, hint2Text);
            hint2Shown = true;
        }
    }

    private void RemoveHint2()
    {
        if (questManager != null && hint2Shown)
        {
            questManager.RemoveCategory(category2Key);
            hint2Shown = false;
        }
    }
}
