using UnityEngine;
using System.Linq;

public class WeaponAttackHintQuest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private Player playerScript;

    [Header("Settings")]
    [SerializeField] private string hintText = "CLICK on an enemy to attack in its general direction. Bow's range is [h]2[/h]. Sword's range is [h]1[/h].";
    [SerializeField] private string categoryKey = "25_weapon_attack_hint";
    [SerializeField] private float delayAfterWeaponAcquired = 10f;

    private Entity playerEntity;
    private bool weaponAcquired = false;
    private float acquisitionTime;
    private bool attackPerformed = false;
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
            {
                playerEntity.inventory.OnItemAdded.AddListener(HandleItemAdded);
                if (playerEntity.inventory.HasAnyItem())
                {
                    OnWeaponAcquired();
                }
            }

            if (playerEntity.agent is AttackerAgent attackerAgent)
            {
                attackerAgent.OnAttackPerformed += HandleAttackPerformed;
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
        if (!weaponAcquired)
        {
            OnWeaponAcquired();
        }
    }

    private void OnWeaponAcquired()
    {
        weaponAcquired = true;
        acquisitionTime = Time.time;
    }

    private void HandleAttackPerformed()
    {
        attackPerformed = true;
        RemoveHint();
        // Once they attack, this specific hint is no longer needed
        this.enabled = false;
    }

    private void Update()
    {
        if (playerEntity == null || attackPerformed || hintShown) return;

        if (weaponAcquired && Time.time >= acquisitionTime + delayAfterWeaponAcquired)
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
