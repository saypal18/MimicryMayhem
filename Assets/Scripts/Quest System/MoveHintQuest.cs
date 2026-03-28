using UnityEngine;

public class MoveHintQuest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private Player playerScript;

    [Header("Settings")]
    [SerializeField] private string hintText = "[h]CLICK[/h] on a neighbouring tile to move";
    [SerializeField] private string categoryKey = "10_move_hint";
    [SerializeField] private float delay = 5f;

    private Entity playerEntity;
    private float startTime;
    private bool hasMoved = false;
    private bool hintShown = false;
    private bool isTracking = false;

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
    }

    private void Initialize(Entity entity)
    {
        playerEntity = entity;
        startTime = Time.time;
        hasMoved = false;
        hintShown = false;
        isTracking = true;
    }

    private void Update()
    {
        if (!isTracking || playerEntity == null) return;

        // Check if player has moved
        if (!hasMoved && playerEntity.moveInfo != null && playerEntity.moveInfo.IsMoving)
        {
            hasMoved = true;
            RemoveHint();
            isTracking = false;
            return;
        }

        // Show hint after delay if not moved
        if (!hasMoved && !hintShown && Time.time >= startTime + delay)
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
