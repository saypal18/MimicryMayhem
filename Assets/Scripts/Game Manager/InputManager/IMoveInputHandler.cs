using UnityEngine;
using UnityEngine.InputSystem;
public interface IMoveInputHandler
{
    void OnGridClick(Vector2Int gridPosition, bool isAttack);
}