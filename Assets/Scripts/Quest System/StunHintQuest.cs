using UnityEngine;

public class StunHintQuest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private Player playerScript;

    [Header("Settings")]
    [SerializeField] private string hintText = "enemies can [h]STUN[/h] you as long as your inventory contains any weapons. [h]DROP[/h] your weapons!";
    [SerializeField] private string categoryKey = "30_stun_hint";

    private Entity playerEntity;
    private bool hintShown = false;

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
        if (playerEntity != null && playerEntity.equippedItem != null)
        {
            playerEntity.equippedItem.OnScroll += HandleScroll;
        }
    }

    private void CleanupListeners()
    {
        if (playerEntity != null && playerEntity.equippedItem != null)
        {
            playerEntity.equippedItem.OnScroll -= HandleScroll;
        }
    }

    private void HandleScroll(int index)
    {
        if (hintShown)
        {
            RemoveHint();
            this.enabled = false;
        }
    }

    private void Update()
    {
        if (playerEntity == null || hintShown) return;

        // Check if player is stunned (controlled)
        if (playerEntity.abilityController != null && playerEntity.abilityController.IsControlled())
        {
            // Trigger: "stunned when not having an equipped item"
            // Get() returns null if the active slot is empty.
            if (playerEntity.equippedItem.Get() == null && playerEntity.inventory.HasAnyItem())
            {
                ShowHint();
            }
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
