using UnityEngine;
using System.Collections.Generic;

public class NextAreaHintQuest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private Player playerScript;

    [Header("Settings")]
    [SerializeField] private string hintText = "You can collect tier 1 and tier 2 bows and swords in this area. move to [h]NEXT AREA[/h], the one on the right to continue";
    [SerializeField] private string categoryKey = "41_next_area_hint";
    [SerializeField] private float delayAfterFull = 10f;

    [Header("Grid Info")]
    [SerializeField] private int grid2Index = 1;

    private Entity playerEntity;
    private bool inventoryWasFull = false;
    private float fullTime;
    private bool hintShown = false;
    private Grid grid2;

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

        var environments = ReflectionUtils.GetFieldValue<List<GameInitializer>>(playerScript, "environments");
        if (environments != null && environments.Count > grid2Index)
        {
            grid2 = environments[grid2Index].grid;
        }
    }

    private void CleanupListeners()
    {
        if (playerEntity != null && playerEntity.pickupHandler != null)
        {
            playerEntity.pickupHandler.OnPickupFailed -= HandlePickupFailed;
        }
    }

    private void HandlePickupFailed(Pickup pickup)
    {
        if (pickup is WeaponPickup && !inventoryWasFull)
        {
            inventoryWasFull = true;
            fullTime = Time.time;
        }
    }

    private void Update()
    {
        if (playerEntity == null) return;

        if (inventoryWasFull && !hintShown)
        {
            if (Time.time >= fullTime + delayAfterFull)
            {
                ShowHint();
            }
        }

        if (hintShown && playerEntity.CurrentGrid == grid2)
        {
            RemoveHint();
            this.enabled = false;
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
