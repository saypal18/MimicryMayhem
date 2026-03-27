using UnityEngine;
using System.Collections.Generic;

public class DoorTile : MonoBehaviour
{
    [SerializeField] private GridPlaceable gridPlaceable;
    public GridPlaceable Placeable => gridPlaceable;

    public GameInitializer parentEnvironment;
    public Grid parentGrid;

    public GameInitializer targetEnvironment;
    public Vector2Int targetDoorPosition;

    public void InitializeDoor(GameInitializer env, Grid grid, Vector2Int position, GameInitializer targetEnv, Vector2Int targetPos)
    {
        parentEnvironment = env;
        parentGrid = grid;
        targetEnvironment = targetEnv;
        targetDoorPosition = targetPos;
        
        if (gridPlaceable == null)
        {
            gridPlaceable = GetComponent<GridPlaceable>();
        }

        gridPlaceable.Initialize(grid, position);
    }
}
