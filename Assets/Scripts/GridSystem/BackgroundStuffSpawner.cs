using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BackgroundStuffConfig
{
    public string name;
    public GameObject prefab;
    public float probability = 1f;
    public Color colorMin = Color.white;
    public Color colorMax = Color.white;
}

[System.Serializable]
public class BackgroundStuffSpawner
{
    [Header("Settings")]
    [SerializeField] private BackgroundStuffConfig[] configs;
    [SerializeField] private float noiseScale = 0.1f;
    [SerializeField] private float threshold = 0.4f;
    [SerializeField] private float density = 0.5f; // Multiplier for how many points to sample per tile area
    [SerializeField] private Vector2 jitter = new Vector2(0.5f, 0.5f);
    [SerializeField] private Transform container;

    private Grid grid;
    private readonly List<GameObject> spawnedObjects = new List<GameObject>();

    public void Initialize(Grid grid)
    {
        this.grid = grid;
    }

    public void SpawnStuff()
    {
        ClearStuff();

        if (grid == null || configs == null || configs.Length == 0)
        {
            return;
        }

        Vector2Int size = grid.Size;
        Vector2 tileSize = grid.TileSize;
        
        float offsetX = Random.Range(0f, 9999f);
        float offsetY = Random.Range(0f, 9999f);

        // We iterate based on grid size but we can sample more or less frequently based on density
        // For a truly "independent" feel, we could use a fixed world area, 
        // but using grid bounds makes sense for this game.
        
        for (float x = 0; x < size.x; x += 1f / density)
        {
            for (float y = 0; y < size.y; y += 1f / density)
            {
                float noise = Mathf.PerlinNoise(x * noiseScale + offsetX, y * noiseScale + offsetY);
                
                if (noise > threshold)
                {
                    BackgroundStuffConfig config = GetRandomConfig();
                    if (config == null || config.prefab == null) continue;

                    // Calculate position relative to grid
                    // grid.GetWorldPosition uses Vector2Int, let's calculate manually for floats
                    Vector3 basePos = grid.GetWorldPosition(Vector2Int.zero);
                    Vector3 worldPos = basePos + new Vector3(x * tileSize.x, y * tileSize.y, 0);
                    
                    // Add jitter
                    worldPos += new Vector3(
                        Random.Range(-jitter.x, jitter.x) * tileSize.x,
                        Random.Range(-jitter.y, jitter.y) * tileSize.y,
                        0
                    );

                    GameObject obj = PoolingEntity.Spawn(config.prefab, worldPos, Quaternion.identity, container);
                    
                    // Apply random color from range
                    SpriteRenderer sr = obj.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.color = Color.Lerp(config.colorMin, config.colorMax, Random.value);
                    }
                    
                    spawnedObjects.Add(obj);
                }
            }
        }
    }

    private BackgroundStuffConfig GetRandomConfig()
    {
        float totalProbability = 0;
        foreach (var c in configs) totalProbability += c.probability;
        if (totalProbability <= 0) return null;

        float rand = Random.value * totalProbability;
        float current = 0;
        foreach (var c in configs)
        {
            current += c.probability;
            if (rand <= current) return c;
        }
        return null;
    }

    public void ClearStuff()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                PoolingEntity.Despawn(obj);
            }
        }
        spawnedObjects.Clear();
    }
}
