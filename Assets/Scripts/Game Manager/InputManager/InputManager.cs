using UnityEngine;
using UnityEngine.InputSystem;
public class InputManager : MonoBehaviour
{
    IMoveInputHandler moveInputHandler;
    IScrollHandler scrollHandler;
    [SerializeField] private Transform mouseArrow;
    public Transform agentTransform { get; set; }
    public Vector2 mousePosition { get; private set; }
    
    private Grid grid;
    private PlayerActionHighlighter highlighter;

    public void InitializeMove(IMoveInputHandler moveInputHandler)
    {
        this.moveInputHandler = moveInputHandler;
    }
    public void InitializeScroll(IScrollHandler scrollHandler)
    {
        this.scrollHandler = scrollHandler;
    }
    public void InitializeClickMap(Grid grid, PlayerActionHighlighter highlighter)
    {
        this.grid = grid;
        this.highlighter = highlighter;
    }

    public void MovePlayer(InputAction.CallbackContext context)
    {
        if (context.performed && grid != null && highlighter != null && highlighter.enabled)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z));
            Vector2Int hoverPos = grid.GetGridPosition(mouseWorldPos);

            if (highlighter.IsValidMoveTile(hoverPos))
            {
                moveInputHandler?.OnGridClick(hoverPos, false);
            }
        }
    }
    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed && grid != null && highlighter != null && highlighter.enabled)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z));
            Vector2Int hoverPos = grid.GetGridPosition(mouseWorldPos);

            if (highlighter.IsValidAttackTile(hoverPos))
            {
                moveInputHandler?.OnGridClick(hoverPos, true);
            }
        }
    }
    public void MouseMove(InputAction.CallbackContext context)
    {
        mousePosition = context.ReadValue<Vector2>();
        moveInputHandler?.OnMouseMove(mousePosition);
    }
    public void Scroll(InputAction.CallbackContext context)
    {
        scrollHandler?.HandleScroll(context);
    }

    /////// apply during play //////////
    private void Update()
    {
        if (mouseArrow == null || moveInputHandler == null || agentTransform == null) return;

        // Formula from AttackerAgent.heuristic
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z));
        Vector3 directionVector = mouseWorldPos - agentTransform.position;

        // Find cardinal direction with smallest angle (formula from AttackerAgent.cs)
        Vector3 cardinalDirection;
        if (Mathf.Abs(directionVector.x) > Mathf.Abs(directionVector.y))
        {
            cardinalDirection = directionVector.x > 0 ? Vector3.right : Vector3.left;
        }
        else
        {
            cardinalDirection = directionVector.y > 0 ? Vector3.up : Vector3.down;
        }

        // Point the arrow's "up" direction towards the cardinal direction
        mouseArrow.up = cardinalDirection;

    }
}