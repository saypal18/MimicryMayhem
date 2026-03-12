using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Allows dragging the UI panel by clicking and dragging on a specific UI element (like a header).
/// Attach this script to the header UI element of the inventory.
/// </summary>
public class UIDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IPointerDownHandler
{
    [Tooltip("The main UI panel that should be moved.")]
    [SerializeField] private RectTransform panelToDrag;

    public Canvas canvas;

    public void OnPointerDown(PointerEventData eventData)
    {
        // Bring the panel to the front immediately when clicked
        if (panelToDrag != null)
        {
            panelToDrag.SetAsLastSibling();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Also bring to front on drag start (redundant but safe)
        if (panelToDrag != null)
        {
            panelToDrag.SetAsLastSibling();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (panelToDrag == null || canvas == null) return;

        // Move the panel based on pointer movement
        // We divide by scaleFactor to ensure movement speed matches the pointer regardless of UI scaling
        panelToDrag.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
}