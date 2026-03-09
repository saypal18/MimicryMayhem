using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PerlinBushPlacer
{
    [Header("References")]
    [SerializeField] private GridPlaceable bushPrefab;
    [SerializeField] private Transform bushParent;

    [Header("Bush Settings")]
    [SerializeField] public PerlinBushConfig perlinConfig = new PerlinBushConfig();

    private Grid grid;

    public void Initialize(Grid grid)
    {
        this.grid = grid;
    }

    public void SpawnAtPosition(Vector2Int position)
    {
        GridPlaceable bush = PoolingEntity.Spawn(bushPrefab, bushParent);
        if (bush != null)
        {
            bush.Initialize(grid, position);
        }
    }

    public void PlaceBushes()
    {
        if (grid == null) return;

        float offsetX = Random.Range(0f, 9999f);
        float offsetY = Random.Range(0f, 9999f);
        float scaleX = Random.Range(perlinConfig.scaleXMin, perlinConfig.scaleXMax);
        float scaleY = Random.Range(perlinConfig.scaleYMin, perlinConfig.scaleYMax);
        float angleRad = Random.Range(perlinConfig.rotationAngleMin, perlinConfig.rotationAngleMax) * Mathf.Deg2Rad;
        float cosA = Mathf.Cos(angleRad);
        float sinA = Mathf.Sin(angleRad);

        int sizeX = grid.Size.x;
        int sizeY = grid.Size.y;

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                // Rotate sampling coords to produce diagonal bush stretching (similar to walls)
                float rx = x * cosA - y * sinA;
                float ry = x * sinA + y * cosA;
                float noise = Mathf.PerlinNoise(rx * scaleX + offsetX, ry * scaleY + offsetY);

                if (noise > perlinConfig.threshold)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    // Place only if no wall exists at this position.
                    // Since bushes are placed right after walls, IsPositionEmpty check is sufficient.
                    if (grid.IsPositionEmpty(pos))
                    {
                        SpawnAtPosition(pos);
                    }
                }
            }
        }
    }
}
