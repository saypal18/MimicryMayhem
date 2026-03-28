using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QuestManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool clearOnStart = true;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private bool highlightBold = true;

    // Using a dictionary to store category -> text
    private Dictionary<string, string> questData = new Dictionary<string, string>();

    // Event triggered when quest data changes
    public UnityEvent<Dictionary<string, string>> OnQuestChanged = new UnityEvent<Dictionary<string, string>>();

    private void Awake()
    {
        if (clearOnStart)
        {
            questData.Clear();
        }
    }

    public void SetQuestText(string category, string text)
    {
        category = category.ToLower().Trim();
        if (string.IsNullOrEmpty(text))
        {
            RemoveCategory(category);
            return;
        }

        text = ApplyHighlights(text);

        if (questData.ContainsKey(category))
        {
            questData[category] = text;
        }
        else
        {
            questData.Add(category, text);
        }

        NotifyChange();
    }

    public void RemoveCategory(string category)
    {
        category = category.ToLower().Trim();
        if (questData.ContainsKey(category))
        {
            questData.Remove(category);
            NotifyChange();
        }
    }

    public void ClearAll()
    {
        questData.Clear();
        NotifyChange();
    }

    private void NotifyChange()
    {
        OnQuestChanged?.Invoke(new Dictionary<string, string>(questData));
    }

    private string ApplyHighlights(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        string openTag = $"<color=#{ColorUtility.ToHtmlStringRGB(highlightColor)}>";
        if (highlightBold) openTag += "<b>";

        string closeTag = "";
        if (highlightBold) closeTag += "</b>";
        closeTag += "</color>";

        return text.Replace("[h]", openTag).Replace("[/h]", closeTag);
    }

    // Helper for specific categories
    public void SetMainQuest(string text) => SetQuestText("main", text);
    public void SetSubQuest(string text) => SetQuestText("sub", text);
    public void SetHint(int index, string text) => SetQuestText($"hint{index}", text);
}
