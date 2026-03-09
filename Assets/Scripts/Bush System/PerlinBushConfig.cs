using UnityEngine;

[System.Serializable]
public class PerlinBushConfig
{
    [Header("Noise Scale — X axis")]
    [Tooltip("Lower = larger blobs along X. Range is randomised each reset.")]
    public float scaleXMin = 0.15f;
    public float scaleXMax = 0.55f;

    [Header("Noise Scale — Y axis")]
    [Tooltip("Lower = larger blobs along Y. Range is randomised each reset.")]
    public float scaleYMin = 0.15f;
    public float scaleYMax = 0.55f;

    [Header("Diagonal Rotation (degrees)")]
    [Tooltip("Rotates the noise sampling coordinates. 0 = axis-aligned, 45 = diagonal streaks.")]
    public float rotationAngleMin = 0f;
    public float rotationAngleMax = 45f;

    [Header("Placement Threshold (0–1)")]
    [Tooltip("Noise values above this threshold become bushes. Higher = fewer bushes.")]
    [Range(0f, 1f)]
    public float threshold = 0.60f;
}
