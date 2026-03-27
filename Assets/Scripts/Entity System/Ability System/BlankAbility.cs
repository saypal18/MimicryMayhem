using UnityEngine;

public class BlankAbility : IAbility
{
    public bool Perform() { return false; }
    public void SetDirection(Vector2Int direction) { }
    public int Range { get; set; }
}