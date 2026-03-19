using UnityEngine;

[System.Serializable]
public class MoveInfo
{
    public Vector2Int CurrentDirection { get; set; } = Vector2Int.zero;
    public bool IsMoving { get; set; } = false;
    public bool IsDashing { get; set; } = false;
}
