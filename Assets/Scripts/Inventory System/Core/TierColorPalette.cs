using UnityEngine;

[CreateAssetMenu(fileName = "TierColorPalette", menuName = "ScriptableObjects/Inventory/TierColorPalette")]
public class TierColorPalette : ScriptableObject
{
    [SerializeField]
    private Color[] tierColors = new Color[]
    {
        Color.black,                  // Tier 0
        Color.white,                  // Tier 1
        Color.green,                  // Tier 2
        Color.blue,                   // Tier 3
        new Color(0.5f, 0f, 0.5f),    // Tier 4 (Purple)
        new Color(1f, 0.5f, 0f)       // Tier 5 (Orange/Gold)
    };

    /// <summary>
    /// Returns the color associated with the given tier (0-5).
    /// </summary>
    /// <param name="tier">The tier index (0 to 5).</param>
    /// <returns>The color for the tier, or Color.black if tier is invalid.</returns>
    public Color GetColorFromTier(int tier)
    {
        if (tierColors == null || tierColors.Length == 0)
        {
            return Color.black;
        }

        int index = Mathf.Clamp(tier, 0, tierColors.Length - 1);
        return tierColors[index];
    }
}
