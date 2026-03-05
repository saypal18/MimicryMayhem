using UnityEngine;

[System.Serializable]
public class Grid
{
    [SerializeField] private Transform startingTilePosition;
    [SerializeField] private Vector2 tileSize;
    [SerializeField] private Vector2Int size;

    public Vector2Int Size => size;
    private Tile[,] tiles;

    public void Initialize()
    {
        tiles = new Tile[size.x, size.y];
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                tiles[x, y] = new Tile(new Vector2Int(x, y));
            }
        }
    }

    public Tile GetTile(Vector2Int gridPosition)
    {
        if (gridPosition.x < 0 || gridPosition.x >= size.x || gridPosition.y < 0 || gridPosition.y >= size.y)
            return null;

        return tiles[gridPosition.x, gridPosition.y];
    }

    public bool IsMovable(Vector2Int gridPosition)
    {
        if (gridPosition.x < 0 || gridPosition.x >= size.x || gridPosition.y < 0 || gridPosition.y >= size.y)
            return false;

        return tiles[gridPosition.x, gridPosition.y].IsMovable;
    }

    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        Vector3 basePos = startingTilePosition != null ? startingTilePosition.position : Vector3.zero;
        return basePos + new Vector3(gridPosition.x * tileSize.x, gridPosition.y * tileSize.y, 0);
    }

    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        Vector3 basePos = startingTilePosition != null ? startingTilePosition.position : Vector3.zero;
        Vector3 relativePos = worldPosition - basePos;

        int x = Mathf.RoundToInt(relativePos.x / tileSize.x);
        int y = Mathf.RoundToInt(relativePos.y / tileSize.y);

        return new Vector2Int(Mathf.Clamp(x, 0, size.x - 1), Mathf.Clamp(y, 0, size.y - 1));
    }

    public Vector2Int GetRandomPosition()
    {
        return new Vector2Int(Random.Range(0, size.x), Random.Range(0, size.y));
    }
}