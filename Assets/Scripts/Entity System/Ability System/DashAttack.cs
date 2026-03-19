using UnityEngine;
using System;
using DG.Tweening;

[Serializable]
public class DashAttack : IAbility
{
    private IEntityMovement movement;
    [SerializeField] private float dashTime;
    [SerializeField] private int blocksToMove = 3;
    private DamageDealer damageDealer;
    private Vector2Int currentDirection = Vector2Int.zero;
    private MoveInfo moveInfo;

    public bool IsDashing => moveInfo != null && moveInfo.IsDashing;

    public void Initialize(DamageDealer dashDamageDealer, EntityMovementFactory movementFactory, GridPlaceable gridPlaceable, MoveInfo moveInfo)
    {
        damageDealer = dashDamageDealer;
        movement = movementFactory.GetMovement(this.GetType());
        movement.Initialize(dashTime, blocksToMove, gridPlaceable, moveInfo);
        this.moveInfo = moveInfo;
    }

    public void SetDirection(Vector2Int direction)
    {
        if (direction != Vector2Int.zero)
        {
            currentDirection = direction;
            if (damageDealer != null)
            {
                damageDealer.direction = direction;
            }
        }
    }

    public bool Perform()
    {
        if (movement == null) return false;

        damageDealer.ResetHitTargets();

        // Already performing a movement
        if (moveInfo.IsMoving) return false;

        // If finished movement but still "dashing" (one-frame linger), clear it
        if (moveInfo.IsDashing)
        {
            DOTween.Kill(moveInfo);
            moveInfo.IsDashing = false;
        }

        if (movement.Move(currentDirection))
        {
            moveInfo.IsDashing = true;
            return true;
        }

        return false;
    }
}
