using UnityEngine;

public interface IRangeDamageColliderAnimation
{
    void Initialize(Grid grid, float travelDuration, float stopDuration, int maxDistanceBlocks);
    void Play(Vector2Int position, Vector2Int direction, GameObject rangeDamageCollider);
}
