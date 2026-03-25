using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GroundTileSpawner
{
    [Header("Settings")]
    [SerializeField] private GameObject groundPrefab;
    [SerializeField] private Sprite[] groundIcons;
    [SerializeField] private Transform container;

    private Grid grid;
    private readonly List<GameObject> spawnedTiles = new List<GameObject>();

    public void Initialize(Grid grid)
    {
        this.grid = grid;
    }

    public void SpawnTiles()
    {
        ClearTiles();

        if (grid == null || groundPrefab == null)
        {
            Debug.LogWarning("GroundTileSpawner: Grid or Prefab is null!");
            return;
        }

        Vector2Int size = grid.Size;
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Vector3 worldPos = grid.GetWorldPosition(pos);
                
                GameObject tile = PoolingEntity.Spawn(groundPrefab, worldPos, Quaternion.identity, container);
                
                // Assign a random icon if available
                if (groundIcons != null && groundIcons.Length > 0)
                {
                    SpriteRenderer sr = tile.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.sprite = groundIcons[Random.Range(0, groundIcons.Length)];
                    }
                }
                
                spawnedTiles.Add(tile);
            }
        }
    }

    public void ClearTiles()
    {
        foreach (GameObject tile in spawnedTiles)
        {
            if (tile != null)
            {
                PoolingEntity.Despawn(tile);
            }
        }
        spawnedTiles.Clear();
    }
}
