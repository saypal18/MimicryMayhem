using UnityEngine;

public interface IDamageColliderAnimation
{
    void Initialize(Grid grid, GameObject swordDamageCollider, float animationDuration, float stopDuration, int damageBlocks);
    void Play(Vector2Int position, Vector2Int direction);
    void UpdateRange(int damageBlocks);
}
