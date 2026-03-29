using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.MLAgents.Policies;
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
    [SerializeField] public bool isKillable = true;
    [SerializeField] public TeamAssignmentStrategy teamAssignmentStrategy = TeamAssignmentStrategy.Alternate;
    [SerializeField] public bool reserveTeamForPlayer = false;
    [SerializeField] public int reservedTeamId = 0;
    [SerializeField] private Vector3 initialScale = Vector3.one;
    [SerializeField] public GameInitializer.AgentType agentType;
    [Header("Inventory Settings")]
    [SerializeField] private bool randomizeInventorySize = false;
    [SerializeField] private int maxInventorySize = 5;



    [Header("Rule Based Weapon Spawning")]
    [SerializeField] private List<Pickup> ruleBasedWeaponPrefabs;
    [SerializeField] private float ruleBasedWeaponSpawnDelay = 5f;

    private int entitiesCount;
    private List<ITick> turnTicks;
    private ITurnManager turnManager;
    private PickupPlacer pickupPlacer;
    private readonly List<Entity> activeEntities = new List<Entity>();
    private List<Pickup> currentWeaponPool;

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
        this.currentWeaponPool = new List<Pickup>(ruleBasedWeaponPrefabs);
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
        SpawnAtPosition(entityPrefab, position, teamId, true, null, entityPrefab.inventory != null ? entityPrefab.inventory.slotCount : 0);
    }

    public Entity SpawnAtPosition(Entity prefab, Vector2Int position, int teamId = 0, bool initializeWeaponProvider = true, int? overrideInventorySize = null, int defaultInventorySize = 0)
    {
        Entity entity = PoolingEntity.Spawn(prefab, entityParent);


        if (entity.inventory != null)
        {
            if (overrideInventorySize.HasValue)
            {
                entity.inventory.slotCount = overrideInventorySize.Value;
            }
            else if (randomizeInventorySize)
            {
                entity.inventory.slotCount = UnityEngine.Random.Range(1, maxInventorySize + 1);
            }
            else if (prefab.inventory != null)
            {
                entity.inventory.slotCount = prefab.inventory.slotCount;
            }
        }

        entity.Initialize(grid, position, movementFactory, turnTicks[teamId], this);

        bool isRuleBased = (agentType == GameInitializer.AgentType.RuleBased) ||
                          (agentType == GameInitializer.AgentType.Randomized && UnityEngine.Random.value > 0.5f);
        entity.agent.isRuleBased = isRuleBased;
        entity.IsKillable = isKillable;

        if (entity.TryGetComponent(out BehaviorParameters bp))
        {
            bp.BehaviorType = isRuleBased ? BehaviorType.HeuristicOnly : BehaviorType.Default;
        }

        if (initializeWeaponProvider)
        {
            entity.ruleBasedWeaponProvider.Initialize(ruleBasedWeaponPrefabs, currentWeaponPool, ruleBasedWeaponSpawnDelay, entity.inventory, grid, pickupPlacer.PickupParent, entity);
        }



        entity.OnDropItemToGrid -= HandleEntityDropItem;
        entity.OnDropItemToGrid += HandleEntityDropItem;

        // Track active entities and auto-remove when despawned
        activeEntities.Add(entity);
        turnManager.RegisterPlayer(teamId);
        if (entity.TryGetComponent(out PoolingEntity poolingEntity))
        {
            poolingEntity.OnDespawning += CreateDespawnHandler(entity, poolingEntity, teamId, defaultInventorySize);
        }


        // if (entity.TryGetComponent(out IMoveInputHandler moveHandler))
        // {
        //     inputManager.InitializeMove(moveHandler);
        // }
        bp.TeamId = teamId;

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
        return entity;
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

            if (reserveTeamForPlayer && numTeams > 1)
            {
                int effectiveTeams = numTeams - 1;
                int strategyIndex = 0;

                if (teamAssignmentStrategy == TeamAssignmentStrategy.Alternate)
                {
                    strategyIndex = i % effectiveTeams;
                }
                else if (teamAssignmentStrategy == TeamAssignmentStrategy.DistinctThenLast)
                {
                    strategyIndex = (i < effectiveTeams) ? i : effectiveTeams - 1;
                }

                // Shift index to avoid reservedTeamId
                teamId = (strategyIndex >= reservedTeamId) ? strategyIndex + 1 : strategyIndex;
                // Clamp to ensure it doesn't exceed numTeams - 1 (in case reservedTeamId was invalidly high)
                if (teamId >= numTeams) teamId = (reservedTeamId == 0) ? 1 : 0;
            }
            else
            {
                if (teamAssignmentStrategy == TeamAssignmentStrategy.Alternate)
                {
                    teamId = i % numTeams;
                }
                else if (teamAssignmentStrategy == TeamAssignmentStrategy.DistinctThenLast)
                {
                    teamId = (i < numTeams) ? i : numTeams - 1;
                }
            }
            
            int defaultSize = entityPrefab.inventory != null ? entityPrefab.inventory.slotCount : 0;
            SpawnAtPosition(entityPrefab, randomPositions[i], teamId, true, null, defaultSize);
        }
    }




    /// <summary>
    /// Returns a self-removing handler: removes the entity from activeEntities
    /// and unsubscribes itself from OnDespawning when the entity is despawned.
    /// This prevents stale delegate accumulation across pool reuse cycles.
    /// </summary>
    private Action CreateDespawnHandler(Entity entity, PoolingEntity poolingEntity, int teamId, int defaultInventorySize)
    {
        Action handler = null;
        handler = () =>
        {
            if (entity.inventory != null)
            {
                entity.inventory.slotCount = defaultInventorySize;
            }
            entity.canBeStunned = true;
            entity.OnDropItemToGrid -= HandleEntityDropItem;
            activeEntities.Remove(entity);
            turnManager.UnregisterPlayer(teamId);
            // poolingEntity.OnDespawning -= handler;
        };
        return handler;
    }


    private void HandleEntityDropItem(Entity dropper, WeaponItem item, Vector2Int position)
    {
        if (pickupPlacer != null)
        {
            pickupPlacer.DropItem(dropper.gameObject, item, position);
        }
    }

    public void RemoveEntitySafely(Entity entity)
    {
        if (activeEntities.Contains(entity))
        {
            entity.canBeStunned = true;
            entity.OnDropItemToGrid -= HandleEntityDropItem;
            activeEntities.Remove(entity);
            turnManager.UnregisterPlayer(entity.TeamId);
        }
    }

    public void AddEntitySafely(Entity entity)
    {
        if (!activeEntities.Contains(entity))
        {
            entity.OnDropItemToGrid -= HandleEntityDropItem;
            entity.OnDropItemToGrid += HandleEntityDropItem;
            activeEntities.Add(entity);
            turnManager.RegisterPlayer(entity.TeamId);
        }
    }

}
