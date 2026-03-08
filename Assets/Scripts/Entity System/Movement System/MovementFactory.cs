using UnityEngine;

[System.Serializable]
public class MovementFactory
{
    public enum MovementType { Snap, Smooth }

    [SerializeField] private MovementType type = MovementType.Snap;
    [SerializeField] private float movementCooldown = 0.2f;

    public IMovement GetMovement(Entity entity)
    {
        switch (type)
        {
            case MovementType.Smooth:
                return new SmoothMovement(entity.transform, movementCooldown);
            case MovementType.Snap:
            default:
                return new SnapMovement(entity.transform, movementCooldown);
        }
    }
}