using UnityEngine;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// A specialized pickup dropped by the boss.
/// When collected by the player, it sets the HasBossKey flag on the player's Entity.
/// </summary>
public class KeyPickup : Pickup
{
    [Header("Audio")]
    [SerializeField] private EventReference keyPickupSoundEvent;

    public override bool Collected(GameObject picker)
    {
        // Don't let the boss pick up its own key (unlikely, but safe)
        if (dropper != null && picker == dropper) return false;

        if (picker.TryGetComponent(out Entity entity))
        {
            // Only the player should benefit from the key (if applicable)
            if (entity.IsPlayer)
            {
                entity.HasBossKey = true;
                Debug.Log("[KeyPickup] Player collected the boss key!");
                PlayKeyPickupSound();
                PoolingEntity.Despawn(gameObject);
                return true;
            }
        }
        return false;
    }

    private void PlayKeyPickupSound()
    {
        if (Trainer.IsTraining) return;
        if (SoundManager.CheckEventNull(keyPickupSoundEvent, "KeyPickup", this)) return;

        EventInstance instance = RuntimeManager.CreateInstance(keyPickupSoundEvent);
        instance.start();
        instance.release();
    }
}
