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

        GameObject spawnedCollider = PoolingEntity.Spawn(rangeColliderPrefab);
        if (spawnedCollider.TryGetComponent(out Root root))
        {
            root.Assign(gridPlaceable.gameObject);
        }
        colliderAnimation.Play(gridPlaceable.Position, currentDirection, spawnedCollider);
        // if (spawnedCollider.TryGetComponent<DamageDealer>(out DamageDealer spawnedDamageDealer))
        // {
        //     spawnedDamageDealer.direction = currentDirection;
        //     if (damageDealer != null)
        //     {
        //         spawnedDamageDealer.tier = damageDealer.tier;
        //         spawnedDamageDealer.applyKnockback = damageDealer.applyKnockback;
        //     }
        // }

        // Vector3 startPos = grid.GetWorldPosition(gridPlaceable.Position);
        // Vector3 endPos = grid.GetWorldPosition(gridPlaceable.Position + currentDirection * maxDistanceBlocks);

        // spawnedCollider.transform.position = startPos;
        // spawnedCollider.SetActive(true);

        // Sequence seq = DOTween.Sequence();
        // seq.Append(spawnedCollider.transform.DOMove(endPos, travelDuration).SetEase(Ease.OutQuad));
        // seq.AppendInterval(stopDuration);
        // seq.OnComplete(() => { PoolingEntity.Despawn(spawnedCollider); });
        // seq.OnKill(() => { PoolingEntity.Despawn(spawnedCollider); });

        return true;
    }

    public void Initialize(Grid grid, DamageDealer rangeDamageDealer)
    {
        this.grid = grid;
        this.damageDealer = rangeDamageDealer;
        this.colliderAnimation.Initialize(grid, travelDuration, stopDuration, maxDistanceBlocks);
    }
}