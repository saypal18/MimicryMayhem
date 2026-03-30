using UnityEngine;
using UnityEngine.UI;

public class WeaponPickup : Pickup
{
    [SerializeField] private WeaponItem weaponItem;
    public WeaponItem GetWeaponItem() => weaponItem;
    
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer weaponRenderer;
    [SerializeField] private SpriteRenderer tierRenderer;
    [SerializeField] private TierColorPalette colorPalette;

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

        // Set the weapon sprite if available
        if (weaponRenderer != null && weaponItem.itemIcon != null)
        {
            weaponRenderer.sprite = weaponItem.itemIcon;
        }

        // Set the tier background color if palette and renderer are available
        if (tierRenderer != null && colorPalette != null)
        {
            tierRenderer.color = colorPalette.GetColorFromTier(weaponItem.tier);
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
