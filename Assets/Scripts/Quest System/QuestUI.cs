using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using DG.Tweening;

public class QuestUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private TextMeshProUGUI questTextBox;

    [Header("Settings")]
    [SerializeField] private string emptyMessage = "";

    [Header("Animation Settings")]
    [SerializeField] private Color colorA = Color.white;
    [SerializeField] private Color colorB = Color.yellow;
    [SerializeField] private float pulseFrequency = 0.5f;
    [SerializeField] private float stopAfterDuration = 5f;

    // Helper class to track state of each quest entry
    private class QuestEntry
    {
        public string text;
        public Color currentColor;
        public Tween colorTween;
        public Tween timerTween;
        public bool isAnimating;
    }

    private Dictionary<string, QuestEntry> entries = new Dictionary<string, QuestEntry>();

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
        foreach (var entry in entries.Values) KillEntryTweens(entry);
    }

    public void UpdateUI(Dictionary<string, string> questData)
    {
        if (questTextBox == null) return;

        bool changed = false;

        // 1. Clean up removed categories
        var keysToRemove = entries.Keys.Where(k => !questData.ContainsKey(k)).ToList();
        foreach (var key in keysToRemove)
        {
            KillEntryTweens(entries[key]);
            entries.Remove(key);
            changed = true;
        }

        // 2. Add or Update individual quest entries
        foreach (var kvp in questData)
        {
            if (string.IsNullOrEmpty(kvp.Value)) continue;

            if (!entries.ContainsKey(kvp.Key))
            {
                // Newly added text! Start tweening it.
                var newEntry = new QuestEntry { text = kvp.Value, currentColor = colorA };
                entries.Add(kvp.Key, newEntry);
                StartPulse(newEntry);
                changed = true;
            }
            else if (entries[kvp.Key].text != kvp.Value)
            {
                // Updated text! Restart its own tween.
                entries[kvp.Key].text = kvp.Value;
                StartPulse(entries[kvp.Key]);
                changed = true;
            }
        }

        // Apply visual updates immediately
        if (changed || entries.Count > 0)
        {
            RebuildText();
        }
        
        if (entries.Count == 0)
        {
            questTextBox.text = emptyMessage;
        }
    }

    private void StartPulse(QuestEntry entry)
    {
        KillEntryTweens(entry);

        entry.currentColor = colorA;
        entry.isAnimating = true;

        // Tween the specific entry's color variable
        entry.colorTween = DOTween.To(() => entry.currentColor, x => entry.currentColor = x, colorB, pulseFrequency)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true);

        // Stop after set duration
        entry.timerTween = DOVirtual.DelayedCall(stopAfterDuration, () => {
            KillEntryTweens(entry);
            RebuildText(); // Final rebuild to remove the tag
        }, false).SetUpdate(true);
    }

    private void KillEntryTweens(QuestEntry entry)
    {
        if (entry.colorTween != null) entry.colorTween.Kill();
        if (entry.timerTween != null) entry.timerTween.Kill();
        entry.colorTween = null;
        entry.timerTween = null;
        entry.currentColor = colorA;
        entry.isAnimating = false;
    }

    private void Update()
    {
        // Rebuild every frame only if at least one entry is still animating
        if (entries.Values.Any(e => e.isAnimating))
        {
            RebuildText();
        }
    }

    private void RebuildText()
    {
        if (questTextBox == null) return;

        var sortedKeys = entries.Keys.OrderBy(k => k).ToList();
        string fullText = "";

        foreach (var key in sortedKeys)
        {
            var entry = entries[key];
            if (entry.isAnimating)
            {
                // Wrap in color tag only during the pulse duration
                string hex = ColorUtility.ToHtmlStringRGBA(entry.currentColor);
                fullText += $"<color=#{hex}>{entry.text}</color>\n\n";
            }
            else
            {
                // Return to normal format (removes the tag completely)
                fullText += $"{entry.text}\n\n";
            }
        }

        questTextBox.text = fullText.TrimEnd('\n');
    }
}




