using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class QuestUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private TextMeshProUGUI questTextBox;

    [Header("Settings")]
    [SerializeField] private string emptyMessage = "";

    private void OnEnable()
    {
        if (questManager != null)
        {
            questManager.OnQuestChanged.AddListener(UpdateUI);
        }
    }

    private void OnDisable()
    {
        if (questManager != null)
        {
            questManager.OnQuestChanged.RemoveListener(UpdateUI);
        }
    }

    public void UpdateUI(Dictionary<string, string> questData)
    {
        if (questTextBox == null) return;

        if (questData.Count == 0)
        {
            questTextBox.text = emptyMessage;
            return;
        }

        // Sort by keys to maintain order (e.g. 01_main, 10_hint1)
        var sortedQuests = questData.OrderBy(kvp => kvp.Key).ToList();
        
        string fullText = "";
        foreach (var kvp in sortedQuests)
        {
            if (string.IsNullOrEmpty(kvp.Value)) continue;
            fullText += kvp.Value + "\n";
        }

        questTextBox.text = fullText.TrimEnd('\n');
    }
}
