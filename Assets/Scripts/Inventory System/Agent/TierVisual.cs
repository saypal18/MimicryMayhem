using UnityEngine.UI;
using UnityEngine;

public class TierVisual : MonoBehaviour
{
    [SerializeField] private Image tierSprite;
    [SerializeField] private TierColorPalette palette;
    [SerializeField] private GameObject cross;

    // tier can be 0, 1, 2, 3, 4, 5
    public void Initialize()
    {
        cross.SetActive(false);
    }
    public void SetTier(int tier)
    {
        if (tierSprite != null && palette != null)
        {
            tierSprite.color = palette.GetColorFromTier(tier);
        }
    }

    public void SetIsOneShot(bool isOneShot)
    {
        if (cross != null)
        {
            cross.SetActive(isOneShot);
        }
    }

}