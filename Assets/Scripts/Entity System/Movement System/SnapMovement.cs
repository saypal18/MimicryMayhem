using UnityEngine;

public class SnapMovement : IMovement
{
    private Transform transform;
    private float cooldown;
    private float lastMoveTime;

    public SnapMovement(Transform transform, float cooldown)
    {
        this.transform = transform;
        this.cooldown = cooldown;
        this.lastMoveTime = -cooldown; // Allow immediate first move
    }

    public bool CanMove()
    {
        return Time.time - lastMoveTime >= cooldown;
    }

    public bool Move(Vector3 initialPosition, Vector3 finalPosition)
    {
        if (!CanMove()) return false;

        transform.position = finalPosition;
        lastMoveTime = Time.time;
        return true;
    }
    public void Snap(Vector3 position)
    {
        transform.position = position;
    }
}
