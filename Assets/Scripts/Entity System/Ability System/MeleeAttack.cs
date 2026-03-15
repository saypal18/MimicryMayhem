using UnityEngine;
using System.Collections;
using System;
[Serializable]
public class MeleeAttack : IAbility
{
    [SerializeField] private Transform swordDamageParent;
    [SerializeField] private GameObject swordDamageCollider;
    [SerializeField] private float animationDuration;
    [SerializeField] private float stopDuration;
    [SerializeField] private GridPlaceable gridPlaceable;
    [SerializeField] private InterfaceReference<IDamageColliderAnimation> _animation;
    private IDamageColliderAnimation animation => _animation.Value;
    [SerializeField] private int damageBlocks; //
    private DamageDealer damageDealer;

    private Vector2Int currentDirection = Vector2Int.zero;
    public void SetDirection(Vector2Int direction)
    {
        if (direction != Vector2Int.zero)
        {
            currentDirection = direction;
            damageDealer.direction = direction;
        }
    }

    public bool Perform()
    {
        // Check if an attack is already in progress
        if (swordDamageCollider == null || swordDamageCollider.activeSelf)
        {
            return false;
        }

        //if (gridPlaceable != null && gridPlaceable.CurrentGrid != null)
        //{
        //    animation.Initialize(gridPlaceable.CurrentGrid);
        //}
        animation.Play(gridPlaceable.Position, currentDirection);

        //StartCoroutine(AttackRoutine());
        return true;
    }

    //private IEnumerator AttackRoutine()
    //{
    //    swordDamageCollider.SetActive(true);
    //    animation.Play(duration, gridPlaceable.Position, currentDirection, damageBlocks, swordDamageCollider);
    //    yield return new WaitForSeconds(duration);
    //    swordDamageCollider.SetActive(false);
    //}
    public void Initialize(Grid grid, DamageDealer meleeDamageDealer)
    {
        swordDamageCollider.SetActive(false);
        animation.Initialize(grid, swordDamageCollider, animationDuration, stopDuration, damageBlocks);
        damageDealer = meleeDamageDealer;
    }

}