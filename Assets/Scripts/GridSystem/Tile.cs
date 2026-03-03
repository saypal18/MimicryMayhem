using UnityEngine;

public class Tile
{
    public Vector2Int Position { get; private set; }
    public bool IsMovable { get; set; }

    public Tile(Vector2Int position, bool isMovable)
    {
        this.Position = position;
        this.IsMovable = isMovable;
    }
}