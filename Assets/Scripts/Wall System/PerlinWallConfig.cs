using UnityEngine;

namespace WallSystem
{
    [System.Serializable]
    public class PerlinWallConfig
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
        [Tooltip("Noise values above this threshold become walls. Higher = fewer walls.")]
        [Range(0f, 1f)]
        public float threshold = 0.50f;

        [Header("Connectivity & Coverage Cap")]
        [Tooltip("Early-stop BFS when (floodFill + walls) covers at least this fraction of the grid …")]
        [Range(0f, 1f)]
        public float minConnectedCoveragePercent = 0.90f;

        [Tooltip("… AND walls are at or below this fraction (companion to minConnectedCoveragePercent).")]
        [Range(0f, 1f)]
        public float maxWallPercentEarlyStop = 0.40f;

        [Tooltip("Hard cap: if walls still exceed this fraction after the connectivity pass, warn and despawn from lowest-noise tiles.")]
        [Range(0f, 1f)]
        public float maxWallCapPercent = 0.50f;
    }
}
