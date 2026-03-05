using UnityEngine;

public interface IGridPlaceable
{
    Vector2Int Position { get; }
    void Move(Vector2Int direction);
}
