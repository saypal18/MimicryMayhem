using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] private Slider gridSizeSlider;
    [SerializeField] private Slider enemyCountSlider;
    [SerializeField] private Toggle randomizeToggle;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI aliveEntityCountText;
    [SerializeField] private TextMeshProUGUI powerLevelText;

    public int GridSize => (int)gridSizeSlider.value;
    public int EnemyCount => (int)enemyCountSlider.value;
    public bool ShouldRandomize => randomizeToggle.isOn;

    public void UpdateStats(int aliveCount, int powerLevel)
    {
        if (aliveEntityCountText != null)
            aliveEntityCountText.text = $"Alive: {aliveCount}";
        if (powerLevelText != null)
            powerLevelText.text = $"Power: {powerLevel}";
    }
}