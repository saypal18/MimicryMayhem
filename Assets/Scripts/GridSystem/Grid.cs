using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public class Grid
{
    [SerializeField] private Transform startingTilePosition;
    [SerializeField] private Vector2 tileSize;
    [SerializeField] private Vector2Int size;

    public Vector2Int Size => size;
    public List<GridPlaceable>[,] tiles;

    public void Initialize()
    {
        PurgeGrid();
        tiles = new List<GridPlaceable>[size.x, size.y];
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                tiles[x, y] = new List<GridPlaceable>();
            }
        }
    }

    public void PurgeGrid()
    {
        if (tiles == null) return;
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                foreach (GridPlaceable gridPlaceable in tiles[x, y])
                {
                    PoolingEntity.Despawn(gridPlaceable.gameObject);
                }
                tiles[x, y].Clear();
            }
        }
    }

    public List<GridPlaceable> GetTile(Vector2Int gridPosition)
    {
        if (gridPosition.x < 0 || gridPosition.x >= size.x || gridPosition.y < 0 || gridPosition.y >= size.y)
            return null;

        return tiles[gridPosition.x, gridPosition.y];
    }

    public bool IsMovable(Vector2Int gridPosition)
    {
        List<GridPlaceable> tile = GetTile(gridPosition);

        if (tile == null) return false;

        foreach (GridPlaceable gridPlaceable in tile)
        {
            if (gridPlaceable.CompareTag("Wall"))
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
        for (int i = 0; i < 5; i++)
        {
            Vector2Int randomPosition = GetRandomPosition();
            if (tiles[randomPosition.x, randomPosition.y].Count == 0)
            {
                return randomPosition;
            }
        }

        List<Vector2Int> emptyPositions = new List<Vector2Int>();
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (tiles[pos.x, pos.y].Count == 0)
                {
                    emptyPositions.Add(pos);
                }
            }
        }

        if (emptyPositions.Count > 0)
        {
            return emptyPositions[Random.Range(0, emptyPositions.Count)];
        }

        return null;
    }

    public List<Vector2Int> GetRandomEmptyPositions(int n)
    {
        List<Vector2Int> emptyPositions = new List<Vector2Int>();
        for (int i = 0; i < n; i++)
        {
            Vector2Int? randomPosition = GetRandomEmptyPosition();
            if (randomPosition != null)
            {
                emptyPositions.Add((Vector2Int)randomPosition);
            }
        }
        return emptyPositions;
    }
}