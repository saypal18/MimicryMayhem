using UnityEngine;
using System.Linq;

public class BowSwordRangeHintQuest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private Player playerScript;

    [Header("Settings")]
    [SerializeField] private string hintText = "Use the long range of [h]BOW[/h] to defeat an enemy using a [h]SWORD[/h].";
    [SerializeField] private string categoryKey = "30_combat_hint_range";
    [SerializeField] private float delayAfterFirstAttack = 10f;

    private Entity playerEntity;
    private bool firstAttackPerformed = false;
    private float firstAttackTime;
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
        
        if (playerEntity != null)
        {
            if (playerEntity.inventory != null)
                playerEntity.inventory.OnItemAdded.AddListener(HandleItemAdded);
            
            if (playerEntity.agent is AttackerAgent attackerAgent)
                attackerAgent.OnAttackPerformed += HandleAttackPerformed;
            
            if (HasSword())
            {
                RemoveHint();
            }
        }
    }

    private void CleanupListeners()
    {
        if (playerEntity != null)
        {
            if (playerEntity.inventory != null)
                playerEntity.inventory.OnItemAdded.RemoveListener(HandleItemAdded);
            
            if (playerEntity.agent is AttackerAgent attackerAgent)
                attackerAgent.OnAttackPerformed -= HandleAttackPerformed;
        }
    }

    private void HandleItemAdded(InventoryItem item, int amount, int index)
    {
        if (item.itemType == ItemType.Sword)
        {
            RemoveHint();
            // Disable this script as it's no longer needed
            this.enabled = false;
        }
    }

    private void HandleAttackPerformed()
    {
        if (!firstAttackPerformed)
        {
            firstAttackPerformed = true;
            firstAttackTime = Time.time;
        }
    }

    private void Update()
    {
        if (playerEntity == null || hintShown || HasSword()) return;

        if (firstAttackPerformed && Time.time >= firstAttackTime + delayAfterFirstAttack)
        {
            ShowHint();
        }
    }

    private bool HasSword()
    {
        if (playerEntity == null || playerEntity.inventory == null) return false;
        return playerEntity.inventory.GetSlots().Any(s => s.item != null && s.item.itemType == ItemType.Sword);
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
