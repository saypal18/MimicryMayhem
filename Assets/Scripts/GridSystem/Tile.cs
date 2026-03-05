using UnityEngine;
using System.Collections.Generic;
public class Tile
{
    public Vector2Int Position { get; private set; }
    public bool IsMovable { get; private set; } = true;
    public List<IGridPlaceable> gridPlaceables { get; private set; }

    public Tile(Vector2Int position)
    {
        this.Position = position;
        gridPlaceables = new();
    }

    public void Add(IGridPlaceable gridPlaceable)
    {
        gridPlaceables.Add(gridPlaceable);
    }

    public void Remove(IGridPlaceable gridPlaceable)
    {
        gridPlaceables.Remove(gridPlaceable);
    }

    public void SetMovable(bool isMovable)
    {
        this.IsMovable = isMovable;
    }
}