using System.Collections.Generic;
using UnityEngine;
using WallSystem;
using Unity.MLAgents.Policies;
using System;
using Unity.MLAgents;

public class GameInitializer : MonoBehaviour
{


    // ── Core references ───────────────────────────────────────────────────────
    [Header("References")]
    [SerializeField] public Grid grid;
    [SerializeField] public EntitySpawner entitySpawner;
    // [SerializeField] public InputManager inputManager;
    [SerializeField] public PickupPlacer pickupPlacer;
    [SerializeField] private WallPlacer wallPlacer;
    [SerializeField] public PerlinBushPlacer bushPlacer;

    // ── Episode settings ──────────────────────────────────────────────────────
    [Header("Episode Settings")]
    [SerializeField] private float maxStepsMultiplier = 40f;
    [SerializeField] private float pickupSpawnIntervalConstant = 4000f;
    [HideInInspector] public int MaxSteps = 1000;
    // [SerializeField] private bool predict = true;
    [HideInInspector] public bool shouldRandomize = true;
    [SerializeField] public ICurriculum curriculum;

    public Action onEnvironmentReset;

    private int stepCount = 0;

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

    public void ResetEnvironment()
    {
        if (curriculum == null)
        {
            curriculum = new BasicBushCurriculum();
            curriculum.Initialize(this);
        }
        curriculum.UpdateCurriculumParameters();

        stepCount = 0;
        if (shouldRandomize)
        {
            grid.RandomizeSize();
        }

        grid.Initialize();

        // Calculate counts based on grid area
        int totalArea = grid.Size.x * grid.Size.y;

        // Dynamic MaxSteps and Pickup Interval based on size

        float averageDimension = (grid.Size.x + grid.Size.y) / 2f;
        MaxSteps = Mathf.RoundToInt(maxStepsMultiplier * averageDimension);

        pickupPlacer.SetInterval(pickupSpawnIntervalConstant / (float)totalArea);

        // Initialise subsystems before spawning anything.
        entitySpawner.Initialize(grid, this);
        bushPlacer.Initialize(grid);
        pickupPlacer.Initialize(grid);
        wallPlacer.Initialize(grid);


        // ── 1. Walls first (so entities/pickups avoid wall tiles) ─────────────
        wallPlacer.PlaceWalls(totalArea);
        bushPlacer.PlaceBushes();

        // ── 2. Entities ────────────────────────────────────────────────────────

        if (shouldRandomize)
        {
            entitySpawner.SetEntityCountByArea(totalArea);
        }
        entitySpawner.SpawnInitialEntities();

        // ── 3. Pickups ────────────────────────────────────────────────────────
        pickupPlacer.SpawnInitialPickups(totalArea);
        onEnvironmentReset?.Invoke();
    }


}
