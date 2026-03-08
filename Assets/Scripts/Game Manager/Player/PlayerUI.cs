using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class PlayerUI
{
    [Header("Controls")]
    [SerializeField] public Slider gridSizeSlider;
    [SerializeField] public Slider enemyCountSlider;
    [SerializeField] public Toggle randomizeToggle;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI aliveEntityCountText;
    [SerializeField] private TextMeshProUGUI powerLevelText;
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] public Button restartButton;
    public int GridSize => (int)gridSizeSlider.value;
    public int EnemyCount => (int)enemyCountSlider.value;
    public bool ShouldRandomize => randomizeToggle.isOn;

    public void UpdateStats(int aliveCount, int powerLevel, int points = 0)
    {
        if (aliveEntityCountText != null)
            aliveEntityCountText.text = $"Alive: {aliveCount}";
        if (powerLevelText != null)
            powerLevelText.text = $"Power: {powerLevel}";
        if (pointsText != null)
            pointsText.text = $"Points: {points}";
    }
}