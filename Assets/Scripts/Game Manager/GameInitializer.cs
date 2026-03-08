using System.Collections.Generic;
using UnityEngine;
using WallSystem;
using Unity.MLAgents.Policies;

public class GameInitializer : MonoBehaviour
{


    // ── Core references ───────────────────────────────────────────────────────
    [Header("References")]
    [SerializeField] private Grid grid;
    [SerializeField] private EntitySpawner entitySpawner;
    [SerializeField] public InputManager inputManager;
    [SerializeField] private PickupPlacer pickupPlacer;
    [SerializeField] private WallPlacer wallPlacer;
    [SerializeField] private PlayerUI playerUI;

    // ── Episode settings ──────────────────────────────────────────────────────
    [Header("Episode Settings")]
    [SerializeField] private float maxStepsMultiplier = 40f;
    [SerializeField] private float pickupSpawnIntervalConstant = 4000f;
    private int MaxSteps = 1000;
    [SerializeField] private bool predict = true;
    bool shouldRandomize;
    Vector2Int gridSize;
    int entityCount;

    private int stepCount = 0;

    private void Start()
    {
        if (predict)
            ResetEnvironment();
    }

    private void FixedUpdate()
    {
        stepCount++;
        pickupPlacer.Tick(Time.fixedDeltaTime);

        // MaxSteps 0 means no time-based reset
        if (MaxSteps > 0 && stepCount >= MaxSteps)
        {
            // End episodes for all active entities, then reset.
            IReadOnlyList<Entity> active = entitySpawner.GetActiveEntities();
            Entity[] snapshot = new Entity[active.Count];
            for (int i = 0; i < active.Count; i++) snapshot[i] = active[i];
            foreach (Entity entity in snapshot)
                entity.ForceEndEpisode();

            ResetEnvironment();
        }
    }

    private void Update()
    {
        if (playerUI != null)
        {
            IReadOnlyList<Entity> activeEntities = entitySpawner.GetActiveEntities();
            int aliveCount = activeEntities.Count;
            int playerPower = 0;

            if (aliveCount > 0)
            {
                // Assuming the first entity is the player
                playerPower = activeEntities[0].damageResolver.power;
            }

            playerUI.UpdateStats(aliveCount, playerPower);
        }
    }

    public void ResetEnvironment()
    {
        stepCount = 0;
        if (shouldRandomize)
        {
            grid.RandomizeSize();
        }

        // // ── Grid Size ────────────────────────────────────────────────────────
        // if (shouldRandomize)
        // {
        //     int sizeValue = playerUI.GridSize;
        //     grid.SetSize(new Vector2Int(sizeValue, sizeValue));
        // }
        // else
        // {
        //     grid.RandomizeSize();
        // }

        grid.Initialize();

        // Calculate counts based on grid area
        int totalArea = grid.Size.x * grid.Size.y;

        // Dynamic MaxSteps and Pickup Interval based on size
        if (playerUI != null)
        {
            // User: Player also sets max steps = 0
            // This disables the time-based reset in FixedUpdate
            MaxSteps = 0;
        }
        else
        {
            float averageDimension = (grid.Size.x + grid.Size.y) / 2f;
            MaxSteps = Mathf.RoundToInt(maxStepsMultiplier * averageDimension);
        }

        pickupPlacer.SetInterval(pickupSpawnIntervalConstant / (float)totalArea);

        // Initialise subsystems before spawning anything.
        entitySpawner.Initialize(grid, inputManager, this);
        pickupPlacer.Initialize(grid);
        wallPlacer.Initialize(grid);

        // ── 1. Walls first (so entities/pickups avoid wall tiles) ─────────────
        wallPlacer.PlaceWalls(totalArea);

        // ── 2. Entities ───────────────────────────────────────────────────────
        if (playerUI != null && !playerUI.ShouldRandomize)
        {
            // If not randomizing, spawn specified count from UI
            entitySpawner.SpawnCount(playerUI.EnemyCount + 1); // +1 for the player
        }
        else
        {
            entitySpawner.SpawnInitialEntities(totalArea);
        }

        // ── 3. Pickups ────────────────────────────────────────────────────────
        pickupPlacer.SpawnInitialPickups(totalArea);

        // ── 4. Predict-mode behaviour assignment ──────────────────────────────
        if (predict)
        {
            IReadOnlyList<Entity> activeEntities = entitySpawner.GetActiveEntities();
            for (int i = 0; i < activeEntities.Count; i++)
            {
                if (!activeEntities[i].TryGetComponent(out BehaviorParameters bp)) continue;

                if (i == 0)
                {
                    bp.BehaviorType = BehaviorType.HeuristicOnly;
                    if (activeEntities[i].TryGetComponent(out IMoveInputHandler handler))
                        inputManager.InitializeMove(handler);
                }
                else
                {
                    bp.BehaviorType = BehaviorType.InferenceOnly;
                }
            }
        }
    }


}
