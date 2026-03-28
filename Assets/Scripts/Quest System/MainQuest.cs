using UnityEngine;

public class MainQuest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private Player playerScript;

    [Header("Quest Text")]
    [SerializeField] private string questText = "[h]ESCAPE[/h] the dungeon";
    [SerializeField] private string categoryKey = "01_main";

    private void OnEnable()
    {
        if (playerScript != null)
        {
            playerScript.OnPlayerSpawned += StartQuest;
        }
    }

    private void OnDisable()
    {
        if (playerScript != null)
        {
            playerScript.OnPlayerSpawned -= StartQuest;
        }
    }

    private void StartQuest(Entity playerEntity)
    {
        if (questManager != null)
        {
            questManager.SetQuestText(categoryKey, questText);
        }
    }
}
