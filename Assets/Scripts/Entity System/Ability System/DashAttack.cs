using UnityEngine;
using System;
using DG.Tweening;

[Serializable]
public class DashAttack : IAbility
{
    private IEntityMovement movement;
    [SerializeField] private float dashTime;
    [SerializeField] private int blocksToMove = 3;
    [SerializeField] private GameObject dashDamageCollider;

    private DamageDealer damageDealer;
    private Vector2Int currentDirection = Vector2Int.zero;

    public void Initialize(DamageDealer dashDamageDealer, EntityMovementFactory movementFactory, GridPlaceable gridPlaceable)
    {
        if (dashDamageCollider != null)
        {
            dashDamageCollider.SetActive(false);
        }
            
        damageDealer = dashDamageDealer;
        movement = movementFactory.GetMovement(this.GetType());
        movement.Initialize(dashTime, blocksToMove, gridPlaceable);
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

        if (dashDamageCollider != null)
        {
            if (dashDamageCollider.activeSelf) return false; // Already dashing
            dashDamageCollider.SetActive(true);
            
            DOVirtual.DelayedCall(dashTime, () => {
                if (dashDamageCollider != null)
                {
                    dashDamageCollider.SetActive(false);
                }
            });
        }
        
        movement.Move(currentDirection);
        return true;
    }
}
