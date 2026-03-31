using System.Collections.Generic;
using UnityEngine;
using WallSystem;
using Unity.MLAgents.Policies;
using System;
using Unity.MLAgents;

public class GameInitializer : MonoBehaviour
{

    [System.Serializable]
    public class DoorLinkConfig
    {
        public Vector2Int localDoorPosition;
        public GameInitializer targetEnvironment;
        public Vector2Int targetDoorPosition;
    }


    // ── Core references ───────────────────────────────────────────────────────
    [Header("References")]
    [SerializeField] public Grid grid;
    [SerializeField] public EntitySpawner entitySpawner;
    // [SerializeField] public InputManager inputManager;
    [SerializeField] public PickupPlacer pickupPlacer;
    [SerializeField] private WallPlacer wallPlacer;
    [SerializeField] public PerlinBushPlacer bushPlacer;
    [SerializeField] public GroundTileSpawner groundTileSpawner;
    [SerializeField] public BackgroundStuffSpawner backgroundStuffSpawner;
    [SerializeField] private BossCreator bossCreator;
    [SerializeField] private VictorySpawner victorySpawner;


    [Header("Audio")]
    [SerializeField] public SoundManager soundManager;

    [Header("Doors")]
    [SerializeField] private DoorTile doorPrefab;
    [SerializeField] public List<DoorLinkConfig> doorLinks = new List<DoorLinkConfig>();
    private List<GameObject> spawnedDoors = new List<GameObject>();
    private GameObject spawnedVictoryTrigger;

    [Header("Team Settings")]
    [SerializeField] public int numTeams = 2;

    [Header("Optimization")]
    [SerializeField] public EntityDistanceActivator distanceActivator = new EntityDistanceActivator();

    // ── Episode settings ──────────────────────────────────────────────────────
    [Header("Episode Settings")]
    [SerializeField] private float maxStepsMultiplier = 40f;
    [SerializeField] private float pickupSpawnIntervalConstant = 4000f;
    [HideInInspector] public int MaxSteps = 1000;
    // [SerializeField] private bool predict = true;
    [HideInInspector] public bool shouldRandomize = true;
    [SerializeField] public ICurriculum curriculum;
    [SerializeField] public TurnManager turnManager;

    public enum AgentType
    {
        MLAgent,
        RuleBased,
        Randomized
    }



    public Action onEnvironmentReset;

    private int stepCount = 0;
    private int minSteps = 100;
    private void FixedUpdate()
    {
        stepCount++;
        pickupPlacer.Tick(Time.fixedDeltaTime);
        distanceActivator.TickActivations(entitySpawner);
        if (stepCount < minSteps) return;
        if (IsOneTeamRemaining() && MaxSteps > 0)
        {
            IReadOnlyList<Entity> active = entitySpawner.GetActiveEntities();
            foreach (Entity entity in active)
            {
                if (entity.agent != null)
                {
                    entity.agent.EndEpisode();
                }
            }

            ResetEnvironment();
            return;

        }

        // MaxSteps 0 means no time-based reset
        if (MaxSteps > 0 && stepCount >= MaxSteps)
        {
            // End episodes for all active entities, then reset.
            IReadOnlyList<Entity> active = entitySpawner.GetActiveEntities();
            Entity[] snapshot = new Entity[active.Count];
            for (int i = 0; i < active.Count; i++) snapshot[i] = active[i];
            foreach (Entity entity in snapshot)
            {

                // entity.agent.EpisodeInterrupted();
                entity.agent.EndEpisode();
            }

            ResetEnvironment();
        }
    }

    private bool IsOneTeamRemaining()
    {
        IReadOnlyList<Entity> active = entitySpawner.GetActiveEntities();
        if (active.Count <= 1) return true;

        int firstTeamId = active[0].TeamId;
        for (int i = 1; i < active.Count; i++)
        {
            if (active[i].TeamId != firstTeamId)
            {
                return false;
            }
        }
        return true;
    }

    private bool banksLoaded = false;

    public void ResetEnvironment()
    {
        if (soundManager != null)
        {
            soundManager.StopBackgroundAudio();

            if (!banksLoaded)
            {
                soundManager.LoadBanks();
                banksLoaded = true;
            }
        }

        numTeams = Mathf.Max(1, numTeams);

        turnManager.Initialize();
        if (curriculum == null)
        {
            curriculum = new BasicBushCurriculum();
            curriculum.Initialize(this);
        }
        curriculum.UpdateCurriculumParameters();

        distanceActivator.ResetEnvironment();
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
        entitySpawner.Initialize(grid, turnManager, pickupPlacer);
        bushPlacer.Initialize(grid);
        pickupPlacer.Initialize(grid);
        wallPlacer.Initialize(grid);
        groundTileSpawner.Initialize(grid);
        groundTileSpawner.SpawnTiles();
        backgroundStuffSpawner.Initialize(grid);
        backgroundStuffSpawner.SpawnStuff();

        SpawnDoors(); // Spawn doors FIRST so they reserve empty tiles on the grid.
        if (victorySpawner != null)
        {
            if (spawnedVictoryTrigger != null) PoolingEntity.Despawn(spawnedVictoryTrigger);
            victorySpawner.Initialize(grid);
            spawnedVictoryTrigger = victorySpawner.SpawnVictoryTrigger();
        }

        // ── 1. Walls first (so entities/pickups avoid wall tiles) ─────────────
        wallPlacer.PlaceWalls(totalArea);
        bushPlacer.PlaceBushes();

        // ── 2. Entities ────────────────────────────────────────────────────────
        if (shouldRandomize)
        {
            entitySpawner.SetEntityCountByArea(totalArea);
        }
        entitySpawner.SpawnInitialEntities();

        if (bossCreator != null)
        {
            bossCreator.Initialize(grid, entitySpawner, pickupPlacer);
            bossCreator.SpawnBoss();
        }


        // ── 3. Pickups ────────────────────────────────────────────────────────
        pickupPlacer.SpawnInitialPickups(totalArea);
        onEnvironmentReset?.Invoke();

        if (soundManager != null)
        {
            StartCoroutine(soundManager.WaitForBanksAndStart());
        }
    }

    private void SpawnDoors()
    {
        foreach (var door in spawnedDoors)
        {
            if (door != null) PoolingEntity.Despawn(door);
        }
        spawnedDoors.Clear();

        if (doorPrefab == null) return;

        foreach (var link in doorLinks)
        {
            Vector3 worldPos = grid.GetWorldPosition(link.localDoorPosition);
            GameObject doorObj = PoolingEntity.Spawn(doorPrefab.gameObject, worldPos, Quaternion.identity, transform);
            DoorTile door = doorObj.GetComponent<DoorTile>();
            door.InitializeDoor(this, grid, link.localDoorPosition, link.targetEnvironment, link.targetDoorPosition);
            spawnedDoors.Add(doorObj);
        }
    }


}
