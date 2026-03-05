using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] private GridPlaceable gridPlaceable;
    [SerializeField] private SurvivorAgent survivorAgent;
    private IMovement movement;
    public void Initialize(Grid grid, Vector2Int startPosition, MovementFactory movementFactory)
    {
        movement = movementFactory.GetMovement(this);
        gridPlaceable.Initialize(grid, startPosition, movement);
        survivorAgent.Initialize(gridPlaceable);
    }
}