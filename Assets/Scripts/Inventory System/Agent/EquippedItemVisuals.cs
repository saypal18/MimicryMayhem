using UnityEngine;

public class EquippedItemVisuals : MonoBehaviour
{
    [SerializeField] private EquippedItem equippedItem;
    [SerializeField] private SpriteRenderer tierSprite;
    [SerializeField] private SpriteRenderer gripSprite;
    [SerializeField] private SpriteRenderer weaponIcon;

    [SerializeField]
    private Color[] tierColors = new Color[]
    {
        Color.white,      // Tier 0
        Color.green,      // Tier 1
        Color.blue,       // Tier 2
        new Color(0.5f, 0f, 0.5f), // Tier 3 (Purple)
        new Color(1f, 0.5f, 0f)    // Tier 4 (Orange/Gold)
    };

    private void Awake()
    {
        if (equippedItem == null)
            equippedItem = GetComponent<EquippedItem>();
    }

    private void Reset()
    {
        if (equippedItem == null)
            equippedItem = GetComponent<EquippedItem>();
    }

    private void Update()
    {
        if (equippedItem == null) return;

        InventoryItem baseItem = equippedItem.Get();
        if (baseItem == null)
        {
            tierSprite.color = Color.white;
            gripSprite.color = Color.white;
            weaponIcon.sprite = null;
            return;
        }

        if (baseItem is WeaponItem weaponItem)
        {
            tierSprite.color = tierColors[Mathf.Clamp(weaponItem.tier - 1, 0, tierColors.Length - 1)];
            gripSprite.color = tierColors[Mathf.Clamp(weaponItem.currentGrip - 1, 0, tierColors.Length - 1)];
            weaponIcon.sprite = weaponItem.itemIcon;
        }
    }
}
