using UnityEngine;

public class WeaponFindHintQuest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private Player playerScript;

    [Header("Settings")]
    [SerializeField] private string hintText = "Find a [h]BOW[/h]";
    [SerializeField] private string categoryKey = "20_weapon_hint";
    [SerializeField] private float delayAfterMove = 10f;

    private Entity playerEntity;
    private bool hasMoved = false;
    private bool hasWeapon = false;
    private bool hintShown = false;
    private float moveTime;

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
        if (playerEntity != null && playerEntity.inventory != null)
        {
            playerEntity.inventory.OnItemAdded.RemoveListener(HandleItemAdded);
        }
    }

    private void Initialize(Entity entity)
    {
        playerEntity = entity;
        hasMoved = false;
        hasWeapon = playerEntity.inventory.HasAnyItem();
        hintShown = false;

        playerEntity.inventory.OnItemAdded.RemoveListener(HandleItemAdded);
        playerEntity.inventory.OnItemAdded.AddListener(HandleItemAdded);
    }

    private void HandleItemAdded(InventoryItem item, int amount, int index)
    {
        hasWeapon = true;
        RemoveHint();
    }

    private void Update()
    {
        if (playerEntity == null || hasWeapon) return;

        // Detect first move
        if (!hasMoved && playerEntity.moveInfo != null && playerEntity.moveInfo.IsMoving)
        {
            hasMoved = true;
            moveTime = Time.time;
            return;
        }

        // Show hint after delay if moved but no weapon
        if (hasMoved && !hasWeapon && !hintShown && Time.time >= moveTime + delayAfterMove)
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
        if (questManager != null)
        {
            questManager.RemoveCategory(categoryKey);
        }
    }
}
