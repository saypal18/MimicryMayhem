using System.Collections.Generic;
using UnityEngine;
using WallSystem;
using Unity.MLAgents.Policies;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private EntitySpawner entitySpawner;
    [SerializeField] public InputManager inputManager;
    [SerializeField] private PickupPlacer pickupPlacer;
    [SerializeField] private WallPlacer wallPlacer;

    private const int MaxSteps = 1000;
    private int stepCount = 0;
    [SerializeField] private bool predict = true;
    private void Start()
    {
        if (predict)
        {
            ResetEnvironment();
        }
    }

    private void FixedUpdate()
    {
        stepCount++;
        if (stepCount >= MaxSteps)
        {
            // End episodes for all active entities, then reset
            IReadOnlyList<Entity> active = entitySpawner.GetActiveEntities();
            // Iterate a snapshot to avoid modification during iteration
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
        grid.Initialize();
        entitySpawner.Initialize(grid, inputManager, this);
        pickupPlacer.Initialize(grid);
        wallPlacer.Initialize(grid);
        entitySpawner.SpawnAtRandomPositions(2);
        pickupPlacer.SpawnAtRandomPositions(5);
        wallPlacer.SpawnAtRandomPositions(4);

        if (predict)
        {
            IReadOnlyList<Entity> activeEntities = entitySpawner.GetActiveEntities();
            for (int i = 0; i < activeEntities.Count; i++)
            {
                if (activeEntities[i].TryGetComponent(out BehaviorParameters bp))
                {
                    if (i == 0)
                    {
                        bp.BehaviorType = BehaviorType.HeuristicOnly;
                        // For predict mode, ensure the human-controlled (heuristic) agent is wired to input
                        if (activeEntities[i].TryGetComponent(out IMoveInputHandler handler))
                        {
                            inputManager.InitializeMove(handler);
                        }
                    }
                    else
                    {
                        bp.BehaviorType = BehaviorType.InferenceOnly;
                    }
                }
            }
        }
    }

}
