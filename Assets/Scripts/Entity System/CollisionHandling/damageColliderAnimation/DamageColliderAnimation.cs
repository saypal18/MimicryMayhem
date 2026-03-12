using UnityEngine;

public interface IDamageColliderAnimation
{
    void Initialize(Grid grid);
    void Play(float duration, Vector2Int position, Vector2Int direction, int damageBlocks, GameObject swordDamageCollider);
}
