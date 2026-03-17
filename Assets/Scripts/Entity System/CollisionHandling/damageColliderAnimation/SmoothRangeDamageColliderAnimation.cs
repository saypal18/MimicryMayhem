using UnityEngine;
using DG.Tweening;

public class SmoothRangeDamageColliderAnimation : MonoBehaviour, IRangeDamageColliderAnimation
{
    private Grid grid;
    private float travelDuration;
    private float stopDuration;
    private int maxDistanceBlocks;

    public void Initialize(Grid grid, float travelDuration, float stopDuration, int maxDistanceBlocks)
    {
        this.grid = grid;
        this.travelDuration = travelDuration;
        this.stopDuration = stopDuration;
        this.maxDistanceBlocks = maxDistanceBlocks;
    }

    public void Play(Vector2Int position, Vector2Int direction, GameObject rangeDamageCollider)
    {
        if (grid == null)
        {
            Debug.LogWarning("SmoothRangeDamageColliderAnimation not initialized with grid!");
            return;
        }


        rangeDamageCollider.SetActive(true);
        Vector3 startPos = grid.GetWorldPosition(position);
        Vector3 endPos = grid.GetWorldPosition(position + direction * maxDistanceBlocks);

        // Reset position to start
        rangeDamageCollider.transform.position = startPos;

        // Ensure collider is enabled when the attack starts
        Collider2D col2d = rangeDamageCollider.GetComponent<Collider2D>();
        if (col2d != null)
        {
            col2d.enabled = true;
        }

        // Animate to end position, then wait, then despawn
        Sequence seq = DOTween.Sequence();
        seq.Append(rangeDamageCollider.transform.DOMove(endPos, travelDuration).SetEase(Ease.OutQuad));
        seq.AppendCallback(() =>
        {
            if (col2d != null)
            {
                col2d.enabled = false;
            }
        });
        seq.AppendInterval(stopDuration);
        seq.OnComplete(() =>
        {
            PoolingEntity.Despawn(rangeDamageCollider);
        });
        seq.OnKill(() =>
        {
            PoolingEntity.Despawn(rangeDamageCollider);
        });
    }
}
