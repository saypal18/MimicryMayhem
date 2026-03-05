using UnityEngine;

public class Entity : MonoBehaviour, IEntity
{
    [SerializeField] private GridPlaceable gridPlaceable;

    public void Initialize(Grid grid, Vector2Int startPosition)
    {
        gridPlaceable.Initialize(grid, startPosition);
    }
}