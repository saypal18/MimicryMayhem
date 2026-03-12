using UnityEngine;
using DG.Tweening;

public class SmoothColliderAnimation : MonoBehaviour, IDamageColliderAnimation
{
    private Grid grid;

    public void Initialize(Grid grid)
    {
        this.grid = grid;
    }

    public void Play(float duration, Vector2Int position, Vector2Int direction, int damageBlocks, GameObject swordDamageCollider)
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
        swordDamageCollider.transform.DOMove(endPos, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => { swordDamageCollider.SetActive(false); })
            .OnKill(() => { swordDamageCollider.SetActive(false); });
    }
}
