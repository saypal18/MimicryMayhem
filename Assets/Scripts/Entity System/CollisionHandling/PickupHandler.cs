using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
[Serializable]

public class PickupHandler
{
    public Action<Pickup> OnPickupCollected;
    public Action<Pickup> OnPickupFailed;
    [SerializeField] private GameObject thisObject;

    [Header("Audio")]
    [SerializeField] private EventReference pickupSoundEvent;
    [SerializeField] private EventReference stealSoundEvent;

    public void Initialize()
    {
        OnPickupCollected = null;
        OnPickupFailed = null;
    }

    public void OnPickup(GameObject other)
    {
        if (thisObject.TryGetComponent(out Entity entity) && entity.IsBoss) return;

        if (other != null && other.TryGetComponent(out Pickup pickup))
        {
            ItemType? pickupItemType = null;
            if (other.TryGetComponent(out WeaponPickup wp))
            {
                var weapon = wp.GetWeaponItem();
                if (weapon != null) pickupItemType = weapon.itemType;
            }

            bool isSteal = pickup.WasDroppedByEntity;

            if (pickup.Collected(thisObject))
            {
                PlayPickupSound(pickupItemType, isSteal);
                OnPickupCollected?.Invoke(pickup);
            }
            else
            {
                OnPickupFailed?.Invoke(pickup);
            }
        }
    }

    private void PlayPickupSound(ItemType? itemType, bool isSteal)
    {

        if (Trainer.IsTraining) return;
        //// FMOD BUG
        //return;
        if (pickupSoundEvent.IsNull) return;
        EventReference eventRef = isSteal ? stealSoundEvent : pickupSoundEvent;
        if (eventRef.IsNull) return;

        if (!itemType.HasValue)
        {
            Debug.LogWarning("[Audio] Pickup collected without an ItemType — consider adding one for this pickup type", thisObject);
            return;
        }

        Entity entity = thisObject.GetComponent<Entity>();
        EventInstance instance = RuntimeManager.CreateInstance(eventRef);
        instance.setParameterByNameWithLabel("ItemType", itemType.Value.ToString());
        instance.setParameterByNameWithLabel("CharacterType", (entity != null && entity.IsPlayer) ? "Player" : "Enemy");
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(thisObject.transform.position));
        instance.start();
        instance.release();
    }
}