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
    private Camera cam;

    public void InitializeMove(IMoveInputHandler moveInputHandler)
    {
        this.moveInputHandler = moveInputHandler;
    }
    public void InitializeScroll(IScrollHandler scrollHandler)
    {
        this.scrollHandler = scrollHandler;
    }
    public void InitializeClickMap(Grid grid, PlayerActionHighlighter highlighter, Camera cam)
    {
        this.grid = grid;
        this.highlighter = highlighter;
        this.cam = cam;
        if (highlighter != null) highlighter.SetInputManager(this);
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed && grid != null && highlighter != null && highlighter.enabled && cam != null)
        {
            Vector3 mouseWorldPos = cam.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -cam.transform.position.z));

            // Try detection on the 'ClickDetection' layer first for a larger hit area
            int clickLayer = LayerMask.NameToLayer("ClickDetection");
            Entity entity = null;
            if (clickLayer != -1)
            {
                Collider2D clickCollider = Physics2D.OverlapPoint(mouseWorldPos, 1 << clickLayer);
                if (clickCollider != null && clickCollider.TryGetComponent(out Root root) && root.GO != null)
                {
                    entity = root.GO.GetComponent<Entity>();
                }
            }

            // Fallback to standard detection if no ClickDetection collider was hit
            if (entity == null)
            {
                Collider2D collider = Physics2D.OverlapPoint(mouseWorldPos);
                if (collider != null) collider.TryGetComponent(out entity);
            }

            if (entity != null)
            {
                // If it's an enemy (not self), perform cardinal attack calculation
                if (entity.transform != agentTransform)
                {
                    moveInputHandler?.OnGridClick(entity.Position, true);
                    return;
                }
            }

            // Fallback to grid-based move if no entity or self clicked
            Vector2Int hoverPos = grid.GetGridPosition(mouseWorldPos);
            if (highlighter.IsAdjacent(hoverPos))
            {
                moveInputHandler?.OnGridClick(hoverPos, false);
            }
        }
    }
    // Make sure this perfectly matches the _Strength in your Material!
    [SerializeField] private float distortionStrength = 1.5f;


    // Your existing input callback
    public void MouseMove(InputAction.CallbackContext context)
    {
        // 1. Read the raw, undistorted mouse position from the hardware
        Vector2 rawMousePosition = context.ReadValue<Vector2>();

        // 2. Run it through our fisheye math
        mousePosition = ApplyFisheyeDistortion(rawMousePosition);

        // 3. Send the CORRECTED position to the rest of your game
    }

    /// <summary>
    /// Takes the raw 2D screen coordinates and warps them using the shader math.
    /// </summary>
    private Vector2 ApplyFisheyeDistortion(Vector2 rawMouse)
    {
        // Convert to UV space [0 to 1]
        Vector2 uv = new Vector2(rawMouse.x / Screen.width, rawMouse.y / Screen.height);

        // Center the UVs [-0.5 to 0.5]
        Vector2 centeredUV = uv - new Vector2(0.5f, 0.5f);

        // Run the math from the shader
        float dist = centeredUV.magnitude;
        float distortion = 1.0f + (distortionStrength * (dist * dist));
        float maxDistortion = 1.0f + (distortionStrength * 0.5f);

        // Calculate the distorted UV
        Vector2 distortedUV = (centeredUV * (distortion / maxDistortion)) + new Vector2(0.5f, 0.5f);

        // Convert back to pixel coordinates and return
        return new Vector2(distortedUV.x * Screen.width, distortedUV.y * Screen.height);
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
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -cam.transform.position.z));
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