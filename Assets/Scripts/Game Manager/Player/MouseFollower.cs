using UnityEngine;

/// <summary>
/// Positions this GameObject at the (fisheye-corrected) mouse world position relative to a
/// player transform, clamped to a configurable max distance.
/// Attach this to the GameObject that is the second target in a CinemachineTargetGroup to
/// achieve a mouse-stretched camera effect.
/// </summary>
public class MouseFollower : MonoBehaviour
{
    [Tooltip("The InputManager that owns the corrected mouse position and camera reference.")]
    [SerializeField] private InputManager inputManager;

    [Tooltip("The player transform to measure distance from.")]
    [SerializeField] private Transform playerTransform;

    [Tooltip("Maximum distance this object can be from the player in world units.")]
    [SerializeField] private float maxDistance = 5f;

    private void Update()
    {
        if (inputManager == null || playerTransform == null) return;

        Camera cam = inputManager.Cam;
        if (cam == null) return;

        Vector2 screenPos = inputManager.mousePosition;

        // Unproject the screen-space mouse position to world space.
        // Uses the same formula as InputManager (orthographic-friendly).
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z)
        );

        // Flatten to the player's plane (ignore any Z/depth drift).
        mouseWorldPos.z = playerTransform.position.z;

        Vector3 offset = mouseWorldPos - playerTransform.position;

        if (offset.magnitude > maxDistance)
            offset = offset.normalized * maxDistance;

        transform.position = playerTransform.position + offset;
    }

    /// <summary>
    /// Convenience method to assign references at runtime (e.g. from Player.cs).
    /// </summary>
    public void Initialize(InputManager inputManager, Transform player)
    {
        this.inputManager = inputManager;
        this.playerTransform = player;
    }
}
