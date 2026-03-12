using UnityEngine.InputSystem;
public interface IMoveInputHandler
{
    void Move(InputAction.CallbackContext context);

    void Attack(InputAction.CallbackContext context);
}