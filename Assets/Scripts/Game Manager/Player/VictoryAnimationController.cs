using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Handles the victory animation sequence using DOTween and Cinemachine 3.
/// </summary>
public class VictoryAnimationController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The main Cinemachine camera used for the victory zoom.")]
    [SerializeField] private CinemachineCamera virtualCamera;
    
    [Tooltip("The point light to intensify during victory.")]
    [SerializeField] private Light2D pointLight;
    
    [Tooltip("The global volume for vignette control.")]
    [SerializeField] private Volume globalVolume;
    
    [Tooltip("The panel to display after the animation completes.")]
    [SerializeField] private GameObject victoryPanel;

    [Header("Cleanup References")]
    [Tooltip("The TurnManager to stop when victory triggers.")]
    [SerializeField] private TurnManager turnManager;

    [Tooltip("The PlayerActionHighlighter to disable.")]
    [SerializeField] private PlayerActionHighlighter highlighter;

    [Tooltip("List of GameObjects to deactivate during the victory sequence.")]
    [SerializeField] private List<GameObject> objectsToDisable = new List<GameObject>();

    [Header("Animation Settings")]
    [Tooltip("Total duration of the victory sequence.")]
    [SerializeField] private float duration = 2f;
    
    [Tooltip("The target orthographic size for the camera zoom. Lower values mean more zoom.")]
    [SerializeField] private float targetZoom = 2f;
    
    [Tooltip("The maximum intensity for the point light.")]
    [SerializeField] private float maxLightIntensity = 10f;
    
    [Tooltip("Ease type that starts slow and speeds up.")]
    [SerializeField] private Ease easeType = Ease.InQuad;

    private Vignette vignette;

    /// <summary>
    /// Replaces Awake/Start for initialization.
    /// </summary>
    public void Initialize()
    {
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet<Vignette>(out vignette);
        }
    }

    /// <summary>
    /// Assigns the player action highlighter at runtime (from Player.cs).
    /// </summary>
    public void SetPlayerActionHighlighter(PlayerActionHighlighter playerHighlighter)
    {
        this.highlighter = playerHighlighter;
    }

    /// <summary>
    /// Triggers the victory animation sequence.
    /// </summary>
    /// <param name="doorTransform">The door to focus the camera on.</param>
    public void PlayVictoryAnimation(Transform doorTransform)
    {
        // 0. Cleanup: Stop turn system and highlights
        if (turnManager != null)
        {
            turnManager.enabled = false;
        }

        if (highlighter != null)
        {
            highlighter.enabled = false; // Triggers OnDisable() which clears highlights
        }

        foreach (var obj in objectsToDisable)
        {
            if (obj != null) obj.SetActive(false);
        }

        // 1. Camera target shift (instantly, letting CM handle focusing)
        if (virtualCamera != null)
        {
            virtualCamera.Follow = doorTransform;
        }

        // 2. Zoom in
        if (virtualCamera != null)
        {
            DOTween.To(() => virtualCamera.Lens.OrthographicSize, 
                       x => virtualCamera.Lens.OrthographicSize = x, 
                       targetZoom, 
                       duration)
                   .SetEase(easeType);
        }

        // 3. Point light intensity increase
        if (pointLight != null)
        {
            DOTween.To(() => pointLight.intensity, 
                       x => pointLight.intensity = x, 
                       maxLightIntensity, 
                       duration)
                   .SetEase(easeType);
        }

        // 4. Decrease vignette effect to 0
        if (vignette != null)
        {
            DOTween.To(() => vignette.intensity.value, 
                       x => vignette.intensity.value = x, 
                       0f, 
                       duration)
                   .SetEase(easeType);
        }

        // After the animation completes, display the victory panel
        DOVirtual.DelayedCall(duration, () =>
        {
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }
        });
    }
}
