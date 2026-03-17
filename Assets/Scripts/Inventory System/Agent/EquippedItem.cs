using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class EquippedItem : MonoBehaviour, IScrollHandler
{
    private int index;
    private InventorySlotHolder inventory;
    public Action<int> OnScroll;
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
    public void Initialize(InventorySlotHolder inventory)
    {
        this.inventory = inventory;
        index = 0;
        OnScroll = null;
    }
    public InventoryItem Get()
    {
        return inventory.GetSlot(index).item;
    }
    public int GetIndex()
    {
        return index;
    }

    public void HandleScroll(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            float scrollValue = context.ReadValue<float>();
            if (scrollValue > 0)
            {
                index = (index + 1) % inventory.slotCount;
            }
            else if (scrollValue < 0)
            {
                index = (index - 1 + inventory.slotCount) % inventory.slotCount;
            }
            OnScroll?.Invoke(index);
        }
    }
    //////// apply during play //////////
    void Update()
    {
        // // put the weapon name, tier and grip in the text
        // WeaponItem item = (WeaponItem)Get();
        // text.text = item.name + "\n" + item.tier + "\n" + item.currentGrip;

        //for grip and tier, color will be based on the tier 
        // for weapon icon, sprite will be based on the item
        InventoryItem baseItem = Get();
        if (baseItem == null)
        {
            tierSprite.color = Color.white;
            gripSprite.color = Color.white;
            weaponIcon.sprite = null;
            return;
        }
        WeaponItem item = (WeaponItem)Get();
        tierSprite.color = tierColors[Mathf.Clamp(item.tier - 1, 0, tierColors.Length - 1)];
        gripSprite.color = tierColors[Mathf.Clamp(item.currentGrip - 1, 0, tierColors.Length - 1)];
        weaponIcon.sprite = item.itemIcon;
    }
}