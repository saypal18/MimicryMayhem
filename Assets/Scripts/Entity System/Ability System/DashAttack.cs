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
    private GridPlaceable gridPlaceable;
    private int _range;
    public int Range
    {
        get => _range;
        set { _range = value; if (movement != null) movement.UpdateRange(value); }
    }

    public bool IsDashing => moveInfo != null && moveInfo.IsDashing;

    public void Initialize(DamageDealer dashDamageDealer, EntityMovementFactory movementFactory, GridPlaceable gridPlaceable, MoveInfo moveInfo)
    {
        damageDealer = dashDamageDealer;
        this.gridPlaceable = gridPlaceable;
        movement = movementFactory.GetMovement(this.GetType());
        movement.Initialize(dashTime, Range > 0 ? Range : blocksToMove, gridPlaceable, moveInfo);
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

    private Animator animator;
    public void SetAnimator(Animator animator)
    {
        this.animator = animator;
    }

    public bool Perform()
    {
        if (movement == null) return false;

        if (animator != null)
        {
            if (currentDirection == Vector2Int.up) animator.SetTrigger("attackUp");
            else if (currentDirection == Vector2Int.down) animator.SetTrigger("attackDown");
            else if (currentDirection == Vector2Int.left) animator.SetTrigger("attackLeft");
            else if (currentDirection == Vector2Int.right) animator.SetTrigger("attackRight");
        }

        damageDealer.ResetHitTargets();
        damageDealer.attackStartPosition = gridPlaceable.Position;

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
