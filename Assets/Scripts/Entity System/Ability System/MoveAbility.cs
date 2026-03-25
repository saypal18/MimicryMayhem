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

    [Header("Audio")]
    [SerializeField] private EventReference gridMovementSoundEvent;

    public void Initialize(EntityMovementFactory movementFactory, GridPlaceable gridPlaceable, MoveInfo moveInfo)
    {
        this.gridPlaceable = gridPlaceable;
        movement = movementFactory.GetMovement(this.GetType());
        movement.Initialize(moveTime, blocksToMove, gridPlaceable, moveInfo);
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

    private void PlayMovementSound()
    {
        if (gridMovementSoundEvent.IsNull) return;

        EventInstance instance = RuntimeManager.CreateInstance(gridMovementSoundEvent);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(gridPlaceable.transform.position));
        instance.start();
        instance.release();
    }
}