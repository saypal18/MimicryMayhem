using System.Collections.Generic;
using UnityEngine;

public sealed class GridPlaceable : MonoBehaviour
{
    private Vector2Int position;
    private Grid grid;
    private PoolingEntity poolingEntity;
    private IMovement movement;

    public Vector2Int Position => position;
    public Grid CurrentGrid => grid;

    public void Initialize(Grid grid, Vector2Int startPosition, IMovement movement = null)
    {
        this.movement = movement;
        if (poolingEntity == null && TryGetComponent(out poolingEntity))
        {
            poolingEntity.OnDespawning += HandleDespawning;
        }

        this.grid = grid;
        this.position = startPosition;

        List<GridPlaceable> startTile = grid.GetTile(position);
        if (startTile != null)
        {
            startTile.Add(this);
            transform.position = grid.GetWorldPosition(position);
        }
    }

    private void HandleDespawning()
    {
        RemoveFromGrid();
    }


    public void Move(Vector2Int direction)
    {
        Vector2Int clampedDirection = new Vector2Int(
            Mathf.Clamp(direction.x, -1, 1),
            Mathf.Clamp(direction.y, -1, 1)
        );
        MoveTo(position + clampedDirection);
    }

    public void Move(Vector2 direction)
    {
        Vector2Int discreteDirection = new Vector2Int(
            Mathf.RoundToInt(Mathf.Clamp(direction.x, -1f, 1f)),
            Mathf.RoundToInt(Mathf.Clamp(direction.y, -1f, 1f))
        );
        MoveTo(position + discreteDirection);
    }

    public void MoveTo(Vector2Int newPosition)
    {
        if (grid == null) return;

        List<GridPlaceable> newTile = grid.GetTile(newPosition);

        if (newTile == null || !grid.IsMovable(newPosition)) return;

        if (!movement.Move(transform.position, grid.GetWorldPosition(newPosition))) return;

        RemoveFromGrid();

        position = newPosition;
        newTile.Add(this);
    }

    public void RemoveFromGrid()
    {
        if (grid == null) return;

        List<GridPlaceable> currentTile = grid.GetTile(position);
        if (currentTile != null)
        {
            currentTile.Remove(this);
        }
    }

}