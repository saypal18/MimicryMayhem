using UnityEngine;
using DG.Tweening;

public class SmoothMovement : IMovement
{
    private Transform transform;
    private float cooldown;
    private float lastMoveTime;

    public SmoothMovement(Transform transform, float cooldown)
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

        transform.DOKill(); // Stop any ongoing movement to prevent conflicts
        transform.position = initialPosition;
        transform.DOMove(finalPosition, cooldown).SetEase(Ease.OutCubic);

        lastMoveTime = Time.time;
        return true;
    }

    public void Snap(Vector3 position)
    {
        transform.DOKill();
        transform.position = position;
    }
}
