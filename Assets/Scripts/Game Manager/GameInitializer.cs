using System.Collections.Generic;
using UnityEngine;
using WallSystem;
using Unity.MLAgents.Policies;

public class GameInitializer : MonoBehaviour
{
    // ── Wall mode ─────────────────────────────────────────────────────────────
    public enum WallMode { Random, PerlinNoise, RandomEachReset }

    [Header("Wall Settings")]
    [SerializeField] private WallMode wallMode = WallMode.RandomEachReset;
    [Tooltip("When WallMode is RandomEachReset: probability (0–1) that a given reset uses Perlin noise instead of scatter.")]
    [Range(0f, 1f)]
    [SerializeField] private float perlinNoiseProbability = 0.5f;
    [SerializeField] private PerlinWallConfig perlinConfig = new PerlinWallConfig();

    [Header("Grid Size Settings")]
    [SerializeField] private int minGridSize = 5;
    [SerializeField] private int maxGridSize = 80;

    [Header("Percentage Settings")]
    [SerializeField] private float entityPercentage = 2f;
    [SerializeField] private float pickupPercentage = 25f;
    [SerializeField] private float wallPercentage = 10f;

    // ── Core references ───────────────────────────────────────────────────────
    [Header("References")]
    [SerializeField] private Grid grid;
    [SerializeField] private EntitySpawner entitySpawner;
    [SerializeField] public InputManager inputManager;
    [SerializeField] private PickupPlacer pickupPlacer;
    [SerializeField] private WallPlacer wallPlacer;

    // ── Episode settings ──────────────────────────────────────────────────────
    [Header("Episode Settings")]
    [SerializeField] private float maxStepsMultiplier = 40f;
    [SerializeField] private float pickupSpawnIntervalConstant = 4000f;
    [SerializeField] private int MaxSteps = 1000;
    [SerializeField] private bool predict = true;

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

        if (stepCount >= MaxSteps)
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

    public void ResetEnvironment()
    {
        stepCount = 0;

        // ── Randomize size before initialization ──────────────────────────────
        int newX = Random.Range(minGridSize, maxGridSize + 1);
        int newY = Random.Range(minGridSize, maxGridSize + 1);
        grid.SetSize(new Vector2Int(newX, newY));

        grid.Initialize();

        // Calculate counts based on grid area
        int totalArea = newX * newY;
        int activeEntityCount = Mathf.Max(2, Mathf.RoundToInt(totalArea * (entityPercentage / 100f)));
        int activePickupCount = Mathf.RoundToInt(totalArea * (pickupPercentage / 100f));
        int activeWallCount = Mathf.RoundToInt(totalArea * (wallPercentage / 100f));

        // Dynamic MaxSteps and Pickup Interval based on size
        float averageDimension = (newX + newY) / 2f;
        MaxSteps = Mathf.RoundToInt(maxStepsMultiplier * averageDimension);
        pickupPlacer.SetInterval(pickupSpawnIntervalConstant / (float)totalArea);

        // Initialise subsystems before spawning anything.
        entitySpawner.Initialize(grid, inputManager, this);
        pickupPlacer.Initialize(grid);
        wallPlacer.Initialize(grid);

        // ── 1. Walls first (so entities/pickups avoid wall tiles) ─────────────
        PlaceWalls(activeWallCount);

        // ── 2. Entities ───────────────────────────────────────────────────────
        entitySpawner.SpawnAtRandomPositions(activeEntityCount);

        // ── 3. Pickups ────────────────────────────────────────────────────────
        pickupPlacer.SpawnAtRandomPositions(activePickupCount);

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

    // ── Wall placement helper ─────────────────────────────────────────────────
    private void PlaceWalls(int scatterWallCount)
    {
        WallMode activeMode = wallMode;

        if (wallMode == WallMode.RandomEachReset)
            activeMode = (Random.value < perlinNoiseProbability) ? WallMode.PerlinNoise : WallMode.Random;

        if (activeMode == WallMode.PerlinNoise)
            wallPlacer.SpawnPerlinNoise(perlinConfig);
        else
            wallPlacer.SpawnAtRandomPositions(scatterWallCount);
    }
}
