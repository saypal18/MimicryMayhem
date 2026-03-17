using UnityEngine;
[CreateAssetMenu(fileName = "NewWeaponItem", menuName = "Inventory/WeaponItem")]

public class WeaponItem : InventoryItem
{
    public int tier;

    [Header("Durability / Grip")]
    //public int maxGrip;
    public int currentGrip;

    // /// <summary>Sound category used as an FMOD parameter for pickup and drop sounds.</summary>
    // [Tooltip("Sound category sent as the FMOD 'ItemType' parameter for pickup and drop sounds.")]
    // public PickupSoundType soundType = PickupSoundType.Other;

    public void Initialize()
    {
        currentGrip = tier;
    }

}