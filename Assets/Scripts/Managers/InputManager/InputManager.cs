using UnityEngine;
using UnityEngine.InputSystem;
public class InputManager : MonoBehaviour
{
    IMoveInputHandler moveInputHandler;
    public void InitializeMove(IMoveInputHandler moveInputHandler)
    {
        this.moveInputHandler = moveInputHandler;
    }
    public void MovePlayer(InputAction.CallbackContext context)
    {
        moveInputHandler.Move(context);
    }

}