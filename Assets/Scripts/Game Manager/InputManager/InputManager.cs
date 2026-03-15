using UnityEngine;
using UnityEngine.InputSystem;
public class InputManager : MonoBehaviour
{
    IMoveInputHandler moveInputHandler;
    IScrollHandler scrollHandler;
    public Vector2 mousePosition { get; private set; }
    public void InitializeMove(IMoveInputHandler moveInputHandler)
    {
        this.moveInputHandler = moveInputHandler;
    }
    public void InitializeScroll(IScrollHandler scrollHandler)
    {
        this.scrollHandler = scrollHandler;
    }
    public void MovePlayer(InputAction.CallbackContext context)
    {
        moveInputHandler?.Move(context);
    }
    public void Attack(InputAction.CallbackContext context)
    {
        moveInputHandler?.Attack(context);
    }
    public void MouseMove(InputAction.CallbackContext context)
    {
        mousePosition = context.ReadValue<Vector2>();
        moveInputHandler?.OnMouseMove(mousePosition);
        // Debug.Log(mousePosition);
    }
    public void Scroll(InputAction.CallbackContext context)
    {
        scrollHandler?.HandleScroll(context);
    }

}