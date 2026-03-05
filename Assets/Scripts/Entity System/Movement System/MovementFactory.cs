using UnityEngine;

[System.Serializable]
public class MovementFactory
{
    [SerializeField] private float movementCooldown = 0.2f;

    public IMovement GetMovement(Entity entity)
    {
        return new SnapMovement(entity.transform, movementCooldown);
    }
}