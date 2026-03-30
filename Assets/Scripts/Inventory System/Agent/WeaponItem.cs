using UnityEngine;
[CreateAssetMenu(fileName = "NewWeaponItem", menuName = "Inventory/WeaponItem")]

public class WeaponItem : InventoryItem
{
    public int tier = 1;

    [Header("Durability / Grip")]
    //public int maxGrip;
    public int currentGrip = 1;
    public int range;

    [Header("Visuals")]
    public GameObject equipPrefab;

    // /// <summary>Sound category used as an FMOD parameter for pickup and drop sounds.</summary>
    // [Tooltip("Sound category sent as the FMOD 'ItemType' parameter for pickup and drop sounds.")]
    // public PickupSoundType soundType = PickupSoundType.Other;



}