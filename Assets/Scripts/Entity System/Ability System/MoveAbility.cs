using System;
using UnityEngine;

[Serializable]
public class MoveAbility : IAbility
{
    protected IEntityMovement movement;
    [SerializeField] private float moveTime;
    [SerializeField] private int blocksToMove = 1;

    public void Initialize(EntityMovementFactory movementFactory, GridPlaceable gridPlaceable, MoveInfo moveInfo)
    {
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
        return movement.Move(currentDirection);
    }
}