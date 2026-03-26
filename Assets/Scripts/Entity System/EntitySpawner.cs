using System;
using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public enum TeamAssignmentStrategy
{
    Alternate,
    DistinctThenLast
}

[System.Serializable]
public class EntitySpawner
{
    [SerializeField] private Entity entityPrefab;
    [SerializeField] private Transform entityParent;
    [SerializeField] public RewardSettings rewardSettings;

    private Grid grid;
    [SerializeField] private InterfaceReference<ITick> _tick;
    [SerializeField] private EntityMovementFactory movementFactory = new();
    private ITick tick => _tick.Value;
    //private GameInitializer gameInitializer;
    [SerializeField] public bool colorize = true;

    [Header("Entity Settings")]
    [SerializeField] public float entityPercentage = 2f;
    [SerializeField] public bool teamsEnabled = false;
    [SerializeField] public TeamAssignmentStrategy teamAssignmentStrategy = TeamAssignmentStrategy.Alternate;
    [SerializeField] private Vector3 initialScale = Vector3.one;
    private int entitiesCount;
    private List<ITick> turnTicks;
    private ITurnManager turnManager;
    private PickupPlacer pickupPlacer;
    private readonly List<Entity> activeEntities = new List<Entity>();

    /// <summary>Number of entities currently alive on the grid.</summary>
    public int ActiveEntityCount => activeEntities.Count;

    /// <summary>Returns true if <paramref name="entity"/> is the only entity left alive.</summary>
    public bool IsLastEntity(Entity entity) =>
        activeEntities.Count == 1 && activeEntities[0] == entity;

    /// <summary>Returns a snapshot of all currently active entities.</summary>
    public IReadOnlyList<Entity> GetActiveEntities() => activeEntities;

    public void Initialize(Grid grid, ITurnManager turnManager, PickupPlacer pickupPlacer)
    {
        this.grid = grid;
        this.turnManager = turnManager;
        this.pickupPlacer = pickupPlacer;
        this.turnTicks = turnManager.GetTeams();
        activeEntities.Clear();
    }

    // public void Initialize(Grid grid, InputManager inputManager, GameInitializer gameInitializer)
    // {
    //     this.grid = grid;
    //     this.inputManager = inputManager;
    //     this.gameInitializer = gameInitializer;
    //     activeEntities.Clear();
    // }

    public void SpawnAtPosition(Vector2Int position, int teamId = 0)
    {
        Entity entity = PoolingEntity.Spawn(entityPrefab, entityParent);
        entity.Initialize(grid, position, movementFactory, turnTicks[teamId]);

        entity.OnDropItemToGrid -= HandleEntityDropItem;
        entity.OnDropItemToGrid += HandleEntityDropItem;

        // Track active entities and auto-remove when despawned
        activeEntities.Add(entity);
        turnManager.RegisterPlayer(teamId);
        if (entity.TryGetComponent(out PoolingEntity poolingEntity))
        {
            poolingEntity.OnDespawning += CreateDespawnHandler(entity, poolingEntity, teamId);
        }

        // if (entity.TryGetComponent(out IMoveInputHandler moveHandler))
        // {
        //     inputManager.InitializeMove(moveHandler);
        // }

        // Apply dynamic Team ID for free-for-all Self-Play
        //if (entity.TryGetComponent(out Unity.MLAgents.Policies.BehaviorParameters bp))
        //{
        //    bp.TeamId = teamId;
        //}
        entity.TeamId = teamId;

        if (colorize)
        {
            Color agentColor = Color.HSVToRGB((teamId * 0.618033988749895f) % 1.0f, 0.8f, 0.9f);
            foreach (var r in entity.GetComponentsInChildren<Renderer>())
            {
                r.material.color = agentColor;
            }
        }
        entity.transform.localScale = initialScale;
    }

    // public void SpawnInitialEntities(int totalArea)
    // {
    //     int count = Mathf.Max(2, Mathf.RoundToInt(totalArea * (entityPercentage / 100f)));
    //     SpawnCount(count);
    // }

    public void SetEntityCountByArea(int totalArea)
    {
        entitiesCount = Mathf.Max(2, Mathf.RoundToInt(totalArea * (entityPercentage / 100f)));
    }
    public void SetEntityCount(int count)
    {
        entitiesCount = count;
    }


    public void SpawnInitialEntities()
    {
        SpawnCount(entitiesCount);
    }

    private void SpawnCount(int count)
    {
        List<Vector2Int> randomPositions = grid.GetRandomEmptyPositions(count);
        int numTeams = turnManager.GetTeamCount();
        for (int i = 0; i < randomPositions.Count; i++)
        {
            int teamId = 0;
            if (teamAssignmentStrategy == TeamAssignmentStrategy.Alternate)
            {
                teamId = i % numTeams;
            }
            else if (teamAssignmentStrategy == TeamAssignmentStrategy.DistinctThenLast)
            {
                teamId = (i < numTeams) ? i : numTeams - 1;
            }
            
            SpawnAtPosition(randomPositions[i], teamId);
        }
    }


    /// <summary>
    /// Returns a self-removing handler: removes the entity from activeEntities
    /// and unsubscribes itself from OnDespawning when the entity is despawned.
    /// This prevents stale delegate accumulation across pool reuse cycles.
    /// </summary>
    private Action CreateDespawnHandler(Entity entity, PoolingEntity poolingEntity, int teamId)
    {
        Action handler = null;
        handler = () =>
        {
            entity.OnDropItemToGrid -= HandleEntityDropItem;
            activeEntities.Remove(entity);
            turnManager.UnregisterPlayer(teamId);
            // poolingEntity.OnDespawning -= handler;
        };
        return handler;
    }

    private void HandleEntityDropItem(WeaponItem item, Vector2Int position)
    {
        if (pickupPlacer != null)
        {
            pickupPlacer.DropItem(item, position);
        }
    }

}
