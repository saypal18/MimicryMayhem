using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public class EntitySpawner
{
    [SerializeField] private Entity entityPrefab;
    [SerializeField] private Transform entityParent;
    private Grid grid;
    private InputManager inputManager;
    public void Initialize(Grid grid, InputManager inputManager)
    {
        this.grid = grid;
        this.inputManager = inputManager;
    }

    public void SpawnAtRandomPosition()
    {
        Vector2Int randomPosition = grid.GetRandomPosition();
        // check if we have any other gridplaceable at this position. if not only then spawn
        List<GameObject> objects = grid.GetObjectsAtPosition(randomPosition);
        if (objects.Count > 0)
        {
            SpawnAtRandomPosition();
            return;
        }
        Entity entity = PoolingEntity.Spawn(entityPrefab);
        entity.Initialize(grid, randomPosition);
        if (entity.TryGetComponent(out IMoveInputHandler moveHandler))
        {
            inputManager.InitializeMove(moveHandler);
        }
    }

    public void SpawnAtRandomPosition(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnAtRandomPosition();
        }
    }
}