using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] private GridPlaceable gridPlaceable;

    public void Initialize(Grid grid, Vector2Int startPosition)
    {
        gridPlaceable.Initialize(grid, startPosition);
    }
}