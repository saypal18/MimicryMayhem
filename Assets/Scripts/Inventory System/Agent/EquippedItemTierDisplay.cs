using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquippedItemTierDisplay : MonoBehaviour
{
    [SerializeField] private EquippedItem equippedItem;
    [SerializeField] private SpriteRenderer weaponIcon;
    [SerializeField] private Transform tierParent;
    [SerializeField] private TierVisual tierPrefab;

    private List<TierVisual> spawnedTierVisuals = new List<TierVisual>();
    private InventoryItem currentItem;
    private int lastTier;
    private int lastGrip;

    private void Awake()
    {
        if (equippedItem == null)
            equippedItem = GetComponent<EquippedItem>();
    }

    private void Update()
    {
        if (equippedItem == null) return;

        InventoryItem newItem = equippedItem.Get();
        int newTier = 0;
        int newGrip = 0;

        if (newItem is WeaponItem weaponItem)
        {
            newTier = weaponItem.tier;
            newGrip = weaponItem.currentGrip;
        }

        if (newItem != currentItem || newTier != lastTier || newGrip != lastGrip)
        {
            UpdateDisplay(newItem);
            currentItem = newItem;
            lastTier = newTier;
            lastGrip = newGrip;
        }
    }

    private void UpdateDisplay(InventoryItem item)
    {
        // Clear existing objects
        foreach (var visual in spawnedTierVisuals)
        {
            if (visual != null)
            {
                visual.transform.SetParent(null);
                PoolingEntity.Despawn(visual.gameObject);
            }
        }
        spawnedTierVisuals.Clear();

        if (item == null)
        {
            weaponIcon.sprite = null;
            return;
        }

        if (item is WeaponItem weaponItem)
        {
            weaponIcon.sprite = weaponItem.itemIcon;

            int tier = weaponItem.tier;
            int grip = weaponItem.currentGrip;

            for (int i = 0; i < tier; i++)
            {
                GameObject tierObj = PoolingEntity.Spawn(tierPrefab.gameObject, tierParent);
                TierVisual visual = tierObj.GetComponent<TierVisual>();
                
                if (visual != null)
                {
                    visual.Initialize();
                    if (i < grip)
                    {
                        visual.SetTier(tier);
                    }
                    else
                    {
                        visual.SetTier(0);
                    }
                    spawnedTierVisuals.Add(visual);
                }
            }
        }
        else
        {
            weaponIcon.sprite = item.itemIcon;
        }
    }
}
