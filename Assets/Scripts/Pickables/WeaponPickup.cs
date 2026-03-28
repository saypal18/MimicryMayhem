using UnityEngine;
using UnityEngine.UI;

public class WeaponPickup : Pickup
{
    [SerializeField] private WeaponItem weaponItem;
    public WeaponItem GetWeaponItem() => weaponItem;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField]
    private Color[] tierColors = new Color[]
    {
        Color.white,      // Tier 0
        Color.green,      // Tier 1
        Color.blue,       // Tier 2
        new Color(0.5f, 0f, 0.5f), // Tier 3 (Purple)
        new Color(1f, 0.5f, 0f)    // Tier 4 (Orange/Gold)
    };

    public override void Initialize(Grid grid, Vector2Int position)
    {
        base.Initialize(grid, position);
        UpdateVisuals();
    }

    public void SetItem(WeaponItem item)
    {
        this.weaponItem = item;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (weaponItem == null) return;

        if (spriteRenderer != null)
        {
            int validTiers = Mathf.Clamp(weaponItem.tier, 1, tierColors.Length);
            spriteRenderer.color = tierColors[validTiers - 1];

            // Optionally set the sprite to the item's icon if it has one
            if (weaponItem.itemIcon != null)
            {
                spriteRenderer.sprite = weaponItem.itemIcon;
            }
        }
    }

    public override bool Collected(GameObject picker)
    {
        if (dropper != null && picker == dropper) return false;
        if (picker.TryGetComponent(out Entity entity))
        {
            // Instantiate a unique copy of the WeaponItem so it has its own Grip value
            WeaponItem instancedWeapon = Instantiate(weaponItem);

            // SortedInventory.AddItem returns true if item was successfully added
            if (entity.inventory.AddItem(instancedWeapon, 1))
            {
                PoolingEntity.Despawn(gameObject);
                return true;
            }
            else
            {
                Destroy(instancedWeapon);
            }
        }
        return false;
    }
}
