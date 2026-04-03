using System;
using UnityEngine;
[Serializable]

public class PickupHandler
{
    public Action<Pickup> OnPickupCollected;
    public Action<Pickup> OnPickupFailed;
    [SerializeField] private GameObject thisObject;

    public bool SuppressNextPickupSound { get; set; }

    public void Initialize()
    {
        OnPickupCollected = null;
        OnPickupFailed = null;
        SuppressNextPickupSound = false;
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
        if (SuppressNextPickupSound)
        {
            SuppressNextPickupSound = false;
            return;
        }

        if (Trainer.IsTraining || SoundManager.Events == null) return;

        if (!itemType.HasValue)
        {
            Debug.LogWarning("[Audio] Pickup collected without an ItemType — consider adding one for this pickup type", thisObject);
            return;
        }

        Entity entity = thisObject.GetComponent<Entity>();
        string characterType = entity != null ? (entity.IsPlayer ? "Player" : entity.IsBoss ? "Boss" : "Enemy") : "Enemy";
        var eventRef = isSteal ? SoundManager.Events.steal : SoundManager.Events.pickup;

        SoundManager.PlayOneShot(eventRef, thisObject.transform.position,
            ("ItemType", itemType.Value.ToString()),
            ("CharacterType", characterType));
    }
}
