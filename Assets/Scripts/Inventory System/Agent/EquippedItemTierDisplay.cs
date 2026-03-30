using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EquippedItemTierDisplay : MonoBehaviour
{
    [SerializeField] private Entity owner;
    [SerializeField] private EquippedItem equippedItem;
    [SerializeField] private SpriteRenderer weaponIcon;
    [SerializeField] private Transform tierParent;
    [SerializeField] private TierVisual tierPrefab;

    [Header("DOTween Shake Settings")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeStrength = 0.05f;

    private List<TierVisual> spawnedTierVisuals = new List<TierVisual>();
    private InventoryItem currentItem;
    private int lastTier;
    private int lastGrip;
    private bool isOneShot;
    private bool isStronger;
    private Tween shakeTween;


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

    public void SetFeedback(bool isOneShot, bool isStronger)
    {
        if (owner != null && owner.IsPlayer) return;

        this.isOneShot = isOneShot;
        this.isStronger = isStronger;

        foreach (var visual in spawnedTierVisuals)
        {
            if (visual != null) visual.SetIsOneShot(isOneShot);
        }

        if (isStronger)
        {
            if (shakeTween == null || !shakeTween.IsActive())
            {
                shakeTween = tierParent.DOShakePosition(shakeDuration, shakeStrength, fadeOut: false).SetLoops(-1);
            }
        }
        else
        {
            if (shakeTween != null)
            {
                shakeTween.Kill();
                shakeTween = null;
            }
            tierParent.localPosition = Vector3.zero;
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
                    visual.SetIsOneShot(isOneShot);
                }
            }
        }
        else
        {
            weaponIcon.sprite = item.itemIcon;
        }
    }
}
