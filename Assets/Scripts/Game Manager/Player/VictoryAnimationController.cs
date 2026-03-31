using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

/// <summary>
/// Handles the victory animation sequence using DOTween and Cinemachine 3.
/// </summary>
public class VictoryAnimationController : MonoBehaviour
{
    [ContextMenu("Test Victory Animation")]
    public void TestVictory()
    {
        PlayVictoryAnimation(this.transform);
    }

    [Header("References")]
    [Tooltip("The main Cinemachine camera used for the victory zoom.")]
    [SerializeField] private CinemachineCamera virtualCamera;

    [Tooltip("The point light to intensify during victory.")]
    [SerializeField] private Light2D pointLight;

    [Tooltip("The global volume for vignette control.")]
    [SerializeField] private Volume globalVolume;

    [Tooltip("The panel to display at the beginning.")]
    [SerializeField] private GameObject victoryPanel;

    [Tooltip("The UI image that goes from 0 to final scale.")]
    [SerializeField] private Image victoryImage;

    [Header("HDR Glow Settings")]
    [Tooltip("Range for Glow Color 1 intensity (Blue/Green).")]
    [SerializeField] private Vector2 glow1IntensityRange = new Vector2(1f, 10f);

    [Tooltip("Range for Glow Color 2 intensity (Blue/Green).")]
    [SerializeField] private Vector2 glow2IntensityRange = new Vector2(1f, 10f);

    [Tooltip("How fast the glow oscillates.")]
    [SerializeField] private float glowOscillationDuration = 1f;

    [Header("Cleanup References")]
    [Tooltip("The TurnManager to stop when victory triggers.")]
    [SerializeField] private TurnManager turnManager;

    [Tooltip("The PlayerActionHighlighter to disable.")]
    [SerializeField] private PlayerActionHighlighter highlighter;

    [Tooltip("List of GameObjects to deactivate during the victory sequence.")]
    [SerializeField] private List<GameObject> objectsToDisable = new List<GameObject>();

    private Material victoryMaterial;
    private Vignette vignette;

    [Header("Animation Settings")]
    [Tooltip("Total duration of the victory sequence.")]
    [SerializeField] private float duration = 2f;

    [Tooltip("The target orthographic size for the camera zoom. Lower values mean more zoom.")]
    [SerializeField] private float targetZoom = 2f;

    [Tooltip("The maximum intensity for the point light.")]
    [SerializeField] private float maxLightIntensity = 10f;

    [Tooltip("Delay before the UI image starts scaling.")]
    [SerializeField] private float imageScaleDelay = 0.5f;

    [Tooltip("Ease type that starts slow and speeds up.")]
    [SerializeField] private Ease easeType = Ease.InQuad;

    [Tooltip("Ease type for UI scaling.")]
    [SerializeField] private Ease scaleEase = Ease.OutBack;

    /// <summary>
    /// Replaces Awake/Start for initialization.
    /// </summary>
    public void Initialize()
    {
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet<Vignette>(out vignette);
        }

        // Pre-instantiate the material to avoid modifying the asset at runtime
        if (victoryImage != null && victoryImage.material != null)
        {
            victoryMaterial = new Material(victoryImage.material);
            victoryImage.material = victoryMaterial;
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

        // 5. Show victory panel after the delay and animate the UI image scale
        if (victoryPanel != null)
        {
            DOVirtual.DelayedCall(imageScaleDelay, () =>
            {
                victoryPanel.SetActive(true);

                if (victoryImage != null)
                {
                    victoryImage.rectTransform.localScale = Vector3.zero;
                    victoryImage.rectTransform.DOScale(Vector3.one, duration)
                                .SetEase(scaleEase);
                    // .OnComplete(() => StartGlowAnimation());
                }
            });
        }
    }

    /// <summary>
    /// Starts the HDR glow oscillation animation on the UI image's material.
    /// </summary>
    private void StartGlowAnimation()
    {
        if (victoryImage == null || victoryMaterial == null) return;

        Material mat = victoryMaterial;

        // Animate Glow 1 (Intensity mapped to Alpha channel in shader)
        DOTween.To(() => mat.GetColor("_GlowColor1").a,
                   x =>
                   {
                       Color c = mat.GetColor("_GlowColor1");
                       c.a = x;
                       mat.SetColor("_GlowColor1", c);
                   }, glow1IntensityRange.y, glowOscillationDuration)
               .From(glow1IntensityRange.x)
               .SetEase(Ease.InOutSine)
               .SetLoops(-1, LoopType.Yoyo);

        // Animate Glow 2 (Intensity mapped to Alpha channel in shader)
        DOTween.To(() => mat.GetColor("_GlowColor2").a,
                   x =>
                   {
                       Color c = mat.GetColor("_GlowColor2");
                       c.a = x;
                       mat.SetColor("_GlowColor2", c);
                   }, glow2IntensityRange.y, glowOscillationDuration)
               .From(glow2IntensityRange.x)
               .SetEase(Ease.InOutSine)
               .SetLoops(-1, LoopType.Yoyo);
    }
}
