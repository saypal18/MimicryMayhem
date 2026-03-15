using UnityEngine;
using UnityEngine.InputSystem;
public interface IMoveInputHandler
{
    void Move(InputAction.CallbackContext context);

    void Attack(InputAction.CallbackContext context);
    void OnMouseMove(Vector2 mousePosition);
}