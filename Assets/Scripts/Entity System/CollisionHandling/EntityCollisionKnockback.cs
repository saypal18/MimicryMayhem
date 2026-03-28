using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
[Serializable]
public class EntityCollisionKnockback : MoveAbility
{
    private AbilityController abilityController;
    private MoveInfo selfMoveInfo;
    private int damageDealerLayer;
    private SortedInventory inventory;

    [Header("Audio")]
    [SerializeField] private EventReference knockbackSoundEvent;

    public void Initialize(EntityMovementFactory movementFactory, GridPlaceable gridPlaceable, MoveInfo moveInfo, AbilityController abilityController, SortedInventory inventory)
    {
        base.Initialize(movementFactory, gridPlaceable, moveInfo);
        this.selfMoveInfo = moveInfo;
        this.abilityController = abilityController;
        this.inventory = inventory;
        this.damageDealerLayer = LayerMask.NameToLayer("DamageDealer");
    }

    public void ApplyKnockback(Entity attacker, int collidedLayer)
    {
        Vector3 selfPos = gridPlaceable.transform.position;
        Vector3 otherPos = attacker.transform.position;
        Vector3 diff = selfPos - otherPos;

        Vector2Int direction = Vector2Int.zero;
        if (Mathf.Abs(diff.x) >= Mathf.Abs(diff.y))
        {
            if (diff.x != 0) direction = new Vector2Int(Mathf.RoundToInt(Mathf.Sign(diff.x)), 0);
        }
        else
        {
            if (diff.y != 0) direction = new Vector2Int(0, Mathf.RoundToInt(Mathf.Sign(diff.y)));
        }

        if (direction != Vector2Int.zero)
        {
            // Stationary entity won't move unless the collided entity is dashing or using melee attack
            if (!selfMoveInfo.IsMoving && !attacker.moveInfo.IsDashing && (attacker.activeAbility == null || !attacker.activeAbility.IsMeleeAttacking))
            {
                return;
            }

            // bool applyStun = ((collidedLayer == damageDealerLayer) || attacker.moveInfo.IsDashing) && inventory.HasAnyItem();
            bool applyStun = (attacker.moveInfo.IsDashing) && inventory.HasAnyItem();
            if (applyStun)
            {
                abilityController.Control(1);
            }
            movement.Move(direction);
            PlayKnockbackSound();
        }
    }

    private void PlayKnockbackSound()
    {
        if (knockbackSoundEvent.IsNull || !SoundManager.CanPlayAudio) return;

        Entity self = gridPlaceable.Entity;
        EventInstance instance = RuntimeManager.CreateInstance(knockbackSoundEvent);
        instance.setParameterByNameWithLabel("CharacterType", (self != null && self.IsPlayer) ? "Player" : "Enemy");
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(gridPlaceable.transform.position));
        instance.start();
        instance.release();
    }
}