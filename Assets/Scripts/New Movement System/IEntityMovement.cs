using UnityEngine;

public interface IEntityMovement
{
    void Initialize(float duration, int blocks, GridPlaceable gridplaceable, MoveInfo moveInfo);
    //void Move(Vector2Int position);
    bool Move(Vector2Int direction);
    void UpdateRange(int blocks);
}