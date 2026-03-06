using System;
using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public class EntitySpawner
{
    [SerializeField] private Entity entityPrefab;
    [SerializeField] private Transform entityParent;
    private Grid grid;
    private InputManager inputManager;
    [SerializeField] private MovementFactory movementFactory;
    private GameInitializer gameInitializer;

    private readonly List<Entity> activeEntities = new List<Entity>();

    /// <summary>Number of entities currently alive on the grid.</summary>
    public int ActiveEntityCount => activeEntities.Count;

    /// <summary>Returns true if <paramref name="entity"/> is the only entity left alive.</summary>
    public bool IsLastEntity(Entity entity) =>
        activeEntities.Count == 1 && activeEntities[0] == entity;

    /// <summary>Returns a snapshot of all currently active entities.</summary>
    public IReadOnlyList<Entity> GetActiveEntities() => activeEntities;

    public void Initialize(Grid grid, InputManager inputManager, GameInitializer gameInitializer)
    {
        this.grid = grid;
        this.inputManager = inputManager;
        this.gameInitializer = gameInitializer;
        activeEntities.Clear();
    }

    public void SpawnAtPosition(Vector2Int position, int teamId = 0)
    {
        Entity entity = PoolingEntity.Spawn(entityPrefab, entityParent);
        entity.Initialize(grid, position, movementFactory, this, gameInitializer);

        // Track active entities and auto-remove when despawned
        activeEntities.Add(entity);
        if (entity.TryGetComponent(out PoolingEntity poolingEntity))
        {
            poolingEntity.OnDespawning += CreateDespawnHandler(entity, poolingEntity);
        }

        if (entity.TryGetComponent(out IMoveInputHandler moveHandler))
        {
            inputManager.InitializeMove(moveHandler);
        }

        // Apply dynamic Team ID for free-for-all Self-Play
        if (entity.TryGetComponent(out Unity.MLAgents.Policies.BehaviorParameters bp))
        {
            bp.TeamId = teamId;
        }
    }

    public void SpawnAtRandomPositions(int count)
    {
        List<Vector2Int> randomPositions = grid.GetRandomEmptyPositions(count);
        int currentTeamId = 0;
        foreach (Vector2Int randomPosition in randomPositions)
        {
            // Each agent gets a unique team ID so they see each other as opponents
            SpawnAtPosition(randomPosition, currentTeamId);
            currentTeamId++;
        }
    }

    /// <summary>
    /// Returns a self-removing handler: removes the entity from activeEntities
    /// and unsubscribes itself from OnDespawning when the entity is despawned.
    /// This prevents stale delegate accumulation across pool reuse cycles.
    /// </summary>
    private Action CreateDespawnHandler(Entity entity, PoolingEntity poolingEntity)
    {
        Action handler = null;
        handler = () =>
        {
            activeEntities.Remove(entity);
            poolingEntity.OnDespawning -= handler;
        };
        return handler;
    }

}
