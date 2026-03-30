using UnityEngine;

public class MimicHintQuest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private Player playerScript;

    [Header("Settings")]
    [SerializeField] private string categoryKey = "80_mimic_hint";
    [SerializeField] [TextArea] private string hintText = "These mimics are not intelligent. They can't be killed. They will spawn lower tier weapons inside them.";
    [SerializeField] private float disappearDelay = 5f;

    private Entity playerEntity;
    private bool hintShown = false;
    private float lastAttackTime = -1f;

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
            playerEntity.damageDealer.OnDamageDealt += HandleDamageDealt;
        }
    }

    private void CleanupListeners()
    {
        if (playerEntity != null && playerEntity.damageDealer != null)
        {
            playerEntity.damageDealer.OnDamageDealt -= HandleDamageDealt;
        }
    }

    private void HandleDamageDealt(Entity victim)
    {
        if (victim == null) return;

        // Check if victim is a rule-based enemy without a weapon
        bool isMimic = victim.agent != null && victim.agent.isRuleBased;
        bool hasNoWeapon = victim.inventory != null && !victim.inventory.HasAnyItem();

        if (isMimic)
        {
            if (hasNoWeapon)
            {
                ShowHint();
                lastAttackTime = Time.time;
            }
            else if (hintShown)
            {
                // If hint is already shown, keep it visible as long as we keep attacking the mimic
                lastAttackTime = Time.time;
            }
        }
    }

    private void Update()
    {
        if (hintShown && Time.time - lastAttackTime > disappearDelay)
        {
            RemoveHint();
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
