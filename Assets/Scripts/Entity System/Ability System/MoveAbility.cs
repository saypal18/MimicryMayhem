using System;
using UnityEngine;

[Serializable]
public class MoveAbility : IAbility
{
    private IEntityMovement movement;
    [SerializeField] private float moveTime;
    [SerializeField] private int blocksToMove = 1;

    public void Initialize(EntityMovementFactory movementFactory, GridPlaceable gridPlaceable)
    {
        // if (movementFactory == null)
        // {
        //     Debug.LogError("movementFactory is null in MoveAbility.Initialize");
        //     return;
        // }

        movement = movementFactory.GetMovement(this.GetType());

        // if (movement == null)
        // {
        //     Debug.LogError($"Movement is null for type {this.GetType()} in MoveAbility.Initialize");
        //     return;
        // }

        movement.Initialize(moveTime, blocksToMove, gridPlaceable);
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
        movement.Move(currentDirection);
        return true;
    }
}