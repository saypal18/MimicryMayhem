using UnityEngine;
using DG.Tweening;

public class SmoothColliderAnimation : MonoBehaviour, IDamageColliderAnimation
{
    private Grid grid;
    private float animationDuration;
    private float stopDuration;
    private int damageBlocks;
    private GameObject swordDamageCollider;

    public void Initialize(Grid grid, GameObject swordDamageCollider, float animationDuration, float stopDuration, int damageBlocks)
    {
        this.grid = grid;
        this.swordDamageCollider = swordDamageCollider;
        this.animationDuration = animationDuration;
        this.stopDuration = stopDuration;
        this.damageBlocks = damageBlocks;
    }

    public void Play(Vector2Int position, Vector2Int direction)
    {
        if (grid == null)
        {
            Debug.LogWarning("SmoothColliderAnimation not initialized with grid!");
            return;
        }
        swordDamageCollider.SetActive(true);
        Vector3 startPos = grid.GetWorldPosition(position);
        Vector3 endPos = grid.GetWorldPosition(position + direction * damageBlocks);

        // Reset position to start
        swordDamageCollider.transform.position = startPos;

        // Animate to end position
        Sequence seq = DOTween.Sequence();
        seq.Append(swordDamageCollider.transform.DOMove(endPos, animationDuration).SetEase(Ease.OutQuad));
        seq.AppendInterval(stopDuration);
        seq.OnComplete(() => { swordDamageCollider.SetActive(false); });
        seq.OnKill(() => { swordDamageCollider.SetActive(false); });
    }

    public void UpdateRange(int range)
    {
        this.damageBlocks = range;
    }
}
