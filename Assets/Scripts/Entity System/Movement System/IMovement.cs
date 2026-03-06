using UnityEngine;

public interface IMovement
{
    /// <summary>Returns true when the movement cooldown has elapsed and a move can be executed.</summary>
    bool CanMove();

    bool Move(Vector3 initialPosition, Vector3 finalPosition);
}