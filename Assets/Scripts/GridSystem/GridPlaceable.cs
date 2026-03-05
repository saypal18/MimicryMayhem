using UnityEngine;

public sealed class GridPlaceable : MonoBehaviour, IGridPlaceable
{
    [SerializeField] private Vector2Int position;
    private Grid grid;
    private PoolingEntity poolingEntity;

    public Vector2Int Position => position;
    public Grid CurrentGrid => grid;

    public void Initialize(Grid grid, Vector2Int startPosition)
    {
        if (poolingEntity == null && TryGetComponent(out poolingEntity))
        {
            poolingEntity.OnDespawning += HandleDespawning;
        }

        this.grid = grid;
        this.position = startPosition;

        Tile startTile = grid.GetTile(position);
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

    public void SetGrid(Grid grid)
    {
        this.grid = grid;
    }

    /// <summary>
    /// Moves the entity by a discrete grid direction (clamped to 1 unit magnitude).
    /// </summary>
    public void Move(Vector2Int direction)
    {
        Vector2Int clampedDirection = new Vector2Int(
            Mathf.Clamp(direction.x, -1, 1),
            Mathf.Clamp(direction.y, -1, 1)
        );
        MoveTo(position + clampedDirection);
    }

    /// <summary>
    /// Moves the entity by a vector direction (clamped to 1 unit magnitude).
    /// </summary>
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

        Tile newTile = grid.GetTile(newPosition);
        if (newTile != null && newTile.IsMovable)
        {
            RemoveFromGrid();

            position = newPosition;
            newTile.Add(this);

            transform.position = grid.GetWorldPosition(position);
            OnMoved();
        }
    }

    public void RemoveFromGrid()
    {
        if (grid != null)
        {
            Tile currentTile = grid.GetTile(position);
            if (currentTile != null)
            {
                currentTile.Remove(this);
            }
        }
    }

    private void OnMoved()
    {
        // Internal movement response
    }
}