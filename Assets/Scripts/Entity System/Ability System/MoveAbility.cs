using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

[Serializable]
public class MoveAbility : IAbility
{
    protected IEntityMovement movement;
    protected GridPlaceable gridPlaceable;
    [SerializeField] private float moveTime;
    [SerializeField] private int blocksToMove = 1;
    public int Range
    {
        get => blocksToMove;
        set { blocksToMove = value; if (movement != null) movement.UpdateRange(value); }
    }

    [Header("Audio")]
    [SerializeField] private EventReference gridMovementSoundEvent;
    private EventInstance movementSoundInstance;

    public void Initialize(EntityMovementFactory movementFactory, GridPlaceable gridPlaceable, MoveInfo moveInfo)
    {
        this.gridPlaceable = gridPlaceable;
        movement = movementFactory.GetMovement(this.GetType());
        movement.Initialize(moveTime, blocksToMove, gridPlaceable, moveInfo);

        ReleaseMovementSound();
    }

    private Vector2Int currentDirection = Vector2Int.zero;
    public void SetDirection(Vector2Int direction)
    {
        if (direction != Vector2Int.zero)
        {
            currentDirection = direction;
        }
    }

    public bool Perform()
    {
        bool moved = movement.Move(currentDirection);
        if (moved) PlayMovementSound();
        return moved;
    }

    private void EnsureMovementSoundInstance()
    {
        if (movementSoundInstance.isValid() || gridMovementSoundEvent.IsNull) return;
        if (!RuntimeManager.HaveAllBanksLoaded) return;

        movementSoundInstance = RuntimeManager.CreateInstance(gridMovementSoundEvent);
    }

    private void PlayMovementSound()
    {
        if (Trainer.IsTraining) return;
        EnsureMovementSoundInstance();
        if (!movementSoundInstance.isValid()) return;

        Entity entity = gridPlaceable.Entity;
        movementSoundInstance.setParameterByNameWithLabel("CharacterType", (entity != null && entity.IsPlayer) ? "Player" : "Enemy");
        movementSoundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gridPlaceable.transform.position));
        movementSoundInstance.start();
    }

    private void ReleaseMovementSound()
    {
        if (movementSoundInstance.isValid())
        {
            movementSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            movementSoundInstance.release();
        }
    }
}