using UnityEngine;
using System;
using DG.Tweening;

[Serializable]
public class RangeAttack : IAbility
{
    [SerializeField] private GameObject rangeColliderPrefab;
    [SerializeField] private float travelDuration;
    [SerializeField] private float stopDuration;
    [SerializeField] private GridPlaceable gridPlaceable;
    [SerializeField] private int maxDistanceBlocks;
    [SerializeField] private InterfaceReference<IRangeDamageColliderAnimation> _animation;
    private IRangeDamageColliderAnimation colliderAnimation => _animation.Value;

    private DamageDealer damageDealer;
    private Grid grid;
    private int _range;
    public int Range 
    { 
        get => _range; 
        set { _range = value; if (colliderAnimation != null) colliderAnimation.UpdateRange(value); } 
    }

    private Vector2Int currentDirection = Vector2Int.zero;

    public void SetDirection(Vector2Int direction)
    {
        if (direction != Vector2Int.zero)
        {
            currentDirection = direction;
            if (damageDealer != null)
                damageDealer.direction = direction;
        }
    }

    public bool Perform()
    {
        if (currentDirection == Vector2Int.zero || grid == null || gridPlaceable == null)
        {
            return false;
        }

        damageDealer.ResetHitTargets();
        damageDealer.attackStartPosition = gridPlaceable.Position;

        GameObject spawnedCollider = PoolingEntity.Spawn(rangeColliderPrefab);
        if (spawnedCollider.TryGetComponent(out Root root))
        {
            root.Assign(gridPlaceable.gameObject);
        }
        colliderAnimation.Play(gridPlaceable.Position, currentDirection, spawnedCollider);

        return true;
    }

    public void Initialize(Grid grid, DamageDealer rangeDamageDealer)
    {
        this.grid = grid;
        this.damageDealer = rangeDamageDealer;
        this.colliderAnimation.Initialize(grid, travelDuration, stopDuration, Range > 0 ? Range : maxDistanceBlocks);
    }
}