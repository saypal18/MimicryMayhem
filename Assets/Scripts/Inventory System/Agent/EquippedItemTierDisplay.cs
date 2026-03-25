using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquippedItemTierDisplay : MonoBehaviour
{
    [SerializeField] private EquippedItem equippedItem;
    [SerializeField] private SpriteRenderer weaponIcon;
    [SerializeField] private Transform tierParent;
    [SerializeField] private GameObject tierPrefab;
    [SerializeField] private Sprite gripSprite;
    [SerializeField] private Sprite nogripsprite;

    private List<GameObject> spawnedTierObjects = new List<GameObject>();
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
        foreach (var obj in spawnedTierObjects)
        {
            if (obj != null)
            {
                obj.transform.SetParent(null);
                PoolingEntity.Despawn(obj);
            }
        }
        spawnedTierObjects.Clear();

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
                GameObject tierObj = PoolingEntity.Spawn(tierPrefab, tierParent);
                spawnedTierObjects.Add(tierObj);

                Image img = tierObj.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = (i < grip) ? gripSprite : nogripsprite;
                }
            }
        }
        else
        {
            weaponIcon.sprite = item.itemIcon;
        }
    }
}
