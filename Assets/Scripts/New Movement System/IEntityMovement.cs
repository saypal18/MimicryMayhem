using UnityEngine;

public interface IEntityMovement
{
    void Initialize(float duration, int blocks, GridPlaceable gridplaceable);
    //void Move(Vector2Int position);
    void Move(Vector2Int direction);
}