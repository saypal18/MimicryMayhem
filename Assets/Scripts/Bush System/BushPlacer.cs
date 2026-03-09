using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BushPlacer
{
    [SerializeField] private GridPlaceable bushPrefab;
    [SerializeField] private Transform bushParent;

    [Header("Bush Settings")]
    [SerializeField] public float bushPercentage = 10f;

    private Grid grid;

    public void Initialize(Grid grid)
    {
        this.grid = grid;
    }

    public void SpawnAtPosition(Vector2Int position)
    {
        GridPlaceable bush = PoolingEntity.Spawn(bushPrefab, bushParent);
        bush.Initialize(grid, position);
    }

    public void SpawnInitialBushes(int totalArea)
    {
        int count = Mathf.RoundToInt(totalArea * (bushPercentage / 100f));
        SpawnAtRandomPositions(count);
    }

    public void SpawnAtRandomPositions(int count)
    {
        List<Vector2Int> randomPositions = grid.GetRandomEmptyPositions(count);
        foreach (Vector2Int randomPosition in randomPositions)
        {
            SpawnAtPosition(randomPosition);
        }
    }
}
