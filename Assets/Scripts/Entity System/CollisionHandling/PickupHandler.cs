using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
[Serializable]

public class PickupHandler
{
    public Action<Pickup> OnPickupCollected;
    [SerializeField] private GameObject thisObject;

    [Header("Audio")]
    [SerializeField] private EventReference pickupSoundEvent;

    public void Initialize()
    {
        OnPickupCollected = null;
    }

    public void OnPickup(GameObject other)
    {
        if (other != null && other.TryGetComponent(out Pickup pickup))
        {
            ItemType? pickupItemType = null;
            if (other.TryGetComponent(out WeaponPickup wp))
            {
                var weapon = wp.GetWeaponItem();
                if (weapon != null) pickupItemType = weapon.itemType;
            }

            if (pickup.Collected(thisObject))
            {
                PlayPickupSound(pickupItemType);
                OnPickupCollected?.Invoke(pickup);
            }
        }
    }

    private void PlayPickupSound(ItemType? itemType)
    {
        if (pickupSoundEvent.IsNull) return;

        if (!itemType.HasValue)
        {
            Debug.LogWarning("[Audio] Pickup collected without an ItemType — consider adding one for this pickup type", thisObject);
            return;
        }

        EventInstance instance = RuntimeManager.CreateInstance(pickupSoundEvent);
        instance.setParameterByNameWithLabel("ItemType", itemType.Value.ToString());
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(thisObject.transform.position));
        instance.start();
        instance.release();
    }
}