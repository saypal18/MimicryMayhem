using UnityEngine;

public interface IAbility
{
    bool Perform();
    void SetDirection(Vector2Int direction);
    int Range { get; set; }
}