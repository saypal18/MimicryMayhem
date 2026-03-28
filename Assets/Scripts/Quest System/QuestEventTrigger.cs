using UnityEngine;

public class QuestEventTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestManager questManager;

    public void SetMainQuest(string text)
    {
        if (questManager != null) questManager.SetMainQuest(text);
    }

    public void SetSubQuest(string text)
    {
        if (questManager != null) questManager.SetSubQuest(text);
    }

    public void SetHint(int index, string text)
    {
        if (questManager != null) questManager.SetHint(index, text);
    }

    public void SetQuestByCategory(string category, string text)
    {
        if (questManager != null) questManager.SetQuestText(category, text);
    }

    public void RemoveCategory(string category)
    {
        if (questManager != null) questManager.RemoveCategory(category);
    }

    public void ClearAllQuests()
    {
        if (questManager != null) questManager.ClearAll();
    }
}
