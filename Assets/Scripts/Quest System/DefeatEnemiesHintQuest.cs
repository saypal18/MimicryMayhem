using UnityEngine;
using System.Linq;

public class DefeatEnemiesHintQuest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private Player playerScript;

    [Header("Settings")]
    [SerializeField] private string hintText = "[h]DEFEAT[/h] [h]3[/h] enemies.";
    [SerializeField] private string categoryKey = "31_combat_hint_defeat";
    [SerializeField] private int enemiesToDefeat = 3;
    [SerializeField] private float delayAfterSword = 30f;

    private Entity playerEntity;
    private bool swordPickedUp = false;
    private float swordAcquisitionTime;
    private bool hintShown = false;
    private int defeatCount = 0;

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

            // If player already has a sword, start timer
            if (HasSword())
            {
                OnSwordPickedUp();
            }
        }

        if (playerEntity != null && playerEntity.agent != null && playerEntity.agent.TryGetComponent(out DamageDealer damageDealer))
        {
            damageDealer.OnDamageDealt += HandleDamageDealt;
        }
    }

    private void CleanupListeners()
    {
        if (playerEntity != null)
        {
            if (playerEntity.inventory != null)
                playerEntity.inventory.OnItemAdded.RemoveListener(HandleItemAdded);

            if (playerEntity.agent != null && playerEntity.agent.TryGetComponent(out DamageDealer damageDealer))
                damageDealer.OnDamageDealt -= HandleDamageDealt;
        }
    }

    private void HandleItemAdded(InventoryItem item, int amount, int index)
    {
        if (item.itemType == ItemType.Sword && !swordPickedUp)
        {
            OnSwordPickedUp();
        }
    }

    private void OnSwordPickedUp()
    {
        swordPickedUp = true;
        swordAcquisitionTime = Time.time;
    }

    private bool HasSword()
    {
        if (playerEntity == null || playerEntity.inventory == null) return false;
        return playerEntity.inventory.GetSlots().Any(s => s.item != null && s.item.itemType == ItemType.Sword);
    }

    private void HandleDamageDealt(Entity victim)
    {
        // if (!swordPickedUp || !hintShown) return;

        defeatCount++;

        if (defeatCount >= enemiesToDefeat)
        {
            RemoveHint();
            this.enabled = false;
        }
    }

    private void Update()
    {
        if (playerEntity == null || hintShown || !swordPickedUp || defeatCount >= enemiesToDefeat) return;

        if (Time.time >= swordAcquisitionTime + delayAfterSword)
        {
            ShowHint();
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
