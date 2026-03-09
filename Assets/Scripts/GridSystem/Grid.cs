using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Linq;
[System.Serializable]
public class Grid
{
    [SerializeField] private Transform startingTilePosition;
    [SerializeField] private Vector2 tileSize;
    private Vector2Int size;

    [Header("Grid Size Settings")]
    [SerializeField] public int minGridSize = 5;
    [SerializeField] public int maxGridSize = 80;
    private Vector2Int sizeForNextMatch;
    public Vector2Int Size => size;
    public Vector2 TileSize => tileSize;
    public List<GridPlaceable>[,] tiles;
    public HashSet<Vector2Int> emptyPositions;

    [SerializeField] private GridBorder border;

    public void SetSize(Vector2Int newSize)
    {
        sizeForNextMatch = newSize;
    }

    public void SetSizeRange(int min, int max)
    {
        minGridSize = min;
        maxGridSize = max;
    }

    public void RandomizeSize()
    {
        int newX = Random.Range(minGridSize, maxGridSize + 1);
        int newY = Random.Range(minGridSize, maxGridSize + 1);
        SetSize(new Vector2Int(newX, newY));
    }

    public void Initialize()
    {
        size = sizeForNextMatch;
        PurgeGrid();
        tiles = new List<GridPlaceable>[size.x, size.y];
        emptyPositions = new HashSet<Vector2Int>();
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                tiles[x, y] = new List<GridPlaceable>();
                emptyPositions.Add(new Vector2Int(x, y));
            }
        }
        border.CreateGridBorder(tileSize, size);
    }

    public void PurgeGrid()
    {
        if (tiles == null) return;
        int oldX = tiles.GetLength(0);
        int oldY = tiles.GetLength(1);
        for (int x = 0; x < oldX; x++)
        {
            for (int y = 0; y < oldY; y++)
            {
                // Iterate backwards to avoid InvalidOperationException 
                // when Despawn triggers RemoveFromGrid() which modifies tiles[x, y].
                for (int i = tiles[x, y].Count - 1; i >= 0; i--)
                {
                    PoolingEntity.Despawn(tiles[x, y][i].gameObject);
                }
                tiles[x, y].Clear();
                emptyPositions?.Add(new Vector2Int(x, y));
            }
        }
    }

    public List<GridPlaceable> GetTile(Vector2Int gridPosition)
    {
        if (gridPosition.x < 0 || gridPosition.x >= size.x || gridPosition.y < 0 || gridPosition.y >= size.y)
            return null;

        return tiles[gridPosition.x, gridPosition.y];
    }

    public void AddToTile(Vector2Int gridPosition, GridPlaceable placeable)
    {
        var tile = GetTile(gridPosition);
        if (tile != null)
        {
            if (tile.Count == 0 && emptyPositions != null)
            {
                emptyPositions.Remove(gridPosition);
            }
            tile.Add(placeable);
        }
    }

    public void RemoveFromTile(Vector2Int gridPosition, GridPlaceable placeable)
    {
        var tile = GetTile(gridPosition);
        if (tile != null)
        {
            tile.Remove(placeable);
            if (tile.Count == 0 && emptyPositions != null)
            {
                emptyPositions.Add(gridPosition);
            }
        }
    }

    public bool IsMovable(Vector2Int gridPosition)
    {
        List<GridPlaceable> tile = GetTile(gridPosition);

        if (tile == null) return false;

        foreach (GridPlaceable gridPlaceable in tile)
        {
            if (gridPlaceable.Type == GridPlaceable.PlaceableType.Wall)
                return false;
        }
        return true;

    }

    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        Vector3 basePos = startingTilePosition.position;
        return basePos + new Vector3(gridPosition.x * tileSize.x, gridPosition.y * tileSize.y, 0);
    }

    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        Vector3 basePos = startingTilePosition.position;
        Vector3 relativePos = worldPosition - basePos;

        int x = Mathf.RoundToInt(relativePos.x / tileSize.x);
        int y = Mathf.RoundToInt(relativePos.y / tileSize.y);

        return new Vector2Int(Mathf.Clamp(x, 0, size.x - 1), Mathf.Clamp(y, 0, size.y - 1));
    }

    public Vector2Int GetRandomPosition()
    {
        return new Vector2Int(Random.Range(0, size.x), Random.Range(0, size.y));
    }
    public bool IsPositionEmpty(Vector2Int gridPosition)
    {
        List<GridPlaceable> tile = GetTile(gridPosition);
        if (tile == null) return false;
        return tile.Count == 0;
    }

    public Vector2Int? GetRandomEmptyPosition()
    {
        if (emptyPositions == null || emptyPositions.Count == 0)
            return null;

        for (int i = 0; i < 5; i++)
        {
            Vector2Int randomPosition = GetRandomPosition();
            if (emptyPositions.Contains(randomPosition))
            {
                return randomPosition;
            }
        }

        int randomIndex = Random.Range(0, emptyPositions.Count);
        return emptyPositions.ElementAt(randomIndex);
    }

    public List<Vector2Int> GetRandomEmptyPositions(int n)
    {
        List<Vector2Int> list = new List<Vector2Int>();
        if (emptyPositions == null || emptyPositions.Count == 0 || n <= 0)
            return list;

        List<Vector2Int> allEmpty = new List<Vector2Int>(emptyPositions);
        for (int i = 0; i < n && allEmpty.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, allEmpty.Count);
            list.Add(allEmpty[randomIndex]);

            // Swap with last and remove to prevent duplicates (O(1))
            int lastIndex = allEmpty.Count - 1;
            allEmpty[randomIndex] = allEmpty[lastIndex];
            allEmpty.RemoveAt(lastIndex);
        }

        return list;
    }
}