using Unity.MLAgents;
using UnityEngine;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;
using System.Collections.Generic;

enum MoveAction
{
    NoAction = 0,
    Up = 1,
    Down = 2,
    Left = 3,
    Right = 4
}

public class SurvivorAgent : Agent, IMoveInputHandler
{
    // ---- references ----
    private GridPlaceable gridPlaceable;
    private DamageResolver damageResolver;
    private PickupHandler pickupHandler;
    private EntitySpawner entitySpawner;
    private Entity thisEntity;
    private GameInitializer gameInitializer;

    // ---- position history ----
    private readonly Queue<Vector2Int> positionHistory = new Queue<Vector2Int>();
    [SerializeField] private int MaxHistoryCount = 5; // Current + 4 previous

    // ---- heuristic state ----
    private int currentHeuristicAction = 0;

    // ---- direction helpers ----
    private static readonly Vector2Int[] Directions = new[]
    {
        Vector2Int.zero,  // NoAction
        Vector2Int.up,    // Up
        Vector2Int.down,  // Down
        Vector2Int.left,  // Left
        Vector2Int.right  // Right
    };

    /// <summary>
    /// Called by Entity.Initialize() to wire up all dependencies.
    /// </summary>
    public void Initialize(
        GridPlaceable gridPlaceable,
        DamageResolver damageResolver,
        PickupHandler pickupHandler,
        EntitySpawner entitySpawner,
        Entity thisEntity,
        GameInitializer gameInitializer)
    {
        this.gridPlaceable = gridPlaceable;
        this.damageResolver = damageResolver;
        this.pickupHandler = pickupHandler;
        this.entitySpawner = entitySpawner;
        this.thisEntity = thisEntity;
        this.gameInitializer = gameInitializer;

        // Wire reward events
        damageResolver.OnDamageTaken += HandleDamageTaken;
        damageResolver.OnDamageDealt += HandleDamageDealt;
        pickupHandler.OnPickupCollected += HandlePickupCollected;

        // Wire sensor references
        if (TryGetComponent(out CustomGridSensorComponent sensorComponent))
        {
            sensorComponent.SetAgentReferences(gridPlaceable, damageResolver, gridPlaceable.CurrentGrid);
        }

        ResetPositionHistory();
    }

    private void ResetPositionHistory()
    {
        positionHistory.Clear();
        if (gridPlaceable != null)
        {
            for (int i = 0; i < MaxHistoryCount; i++)
            {
                positionHistory.Enqueue(gridPlaceable.Position);
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        ResetPositionHistory();
    }

    // ---- reward handlers ----

    private void HandleDamageTaken()
    {
        AddReward(-0.5f);
        EndEpisode();
        // Trigger the full despawn chain: OnDespawning → GridPlaceable.RemoveFromGrid
        // → EntitySpawner.activeEntities auto-remove via CreateDespawnHandler
        PoolingEntity.Despawn(gameObject);
    }

    private void HandleDamageDealt()
    {
        AddReward(1.0f);

        // Check win condition: are we the last entity on the grid?
        if (entitySpawner != null && entitySpawner.IsLastEntity(thisEntity))
        {
            AddReward(1f);
            EndEpisode();
            if (gameInitializer != null)
            {
                gameInitializer.ResetEnvironment();
            }
        }
    }

    private void HandlePickupCollected(Pickup pickup)
    {
        AddReward(0.1f);
    }

    // ---- action masking ----

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        if (gridPlaceable == null) return;

        Grid grid = gridPlaceable.CurrentGrid;
        if (grid == null) return;

        Vector2Int pos = gridPlaceable.Position;

        // MoveAction indices: 0=NoAction,1=Up,2=Down,3=Left,4=Right
        for (int action = 1; action <= 4; action++)
        {
            Vector2Int neighbor = pos + Directions[action];
            if (!grid.IsMovable(neighbor))
            {
                actionMask.SetActionEnabled(0, action, false);
            }
        }
    }

    // ---- decision loop ----

    private void Update()
    {
        // Request a new decision only when the movement cooldown has elapsed.
        if (gridPlaceable != null && gridPlaceable.CanMove())
        {
            RequestDecision();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (gridPlaceable == null || gridPlaceable.CurrentGrid == null)
        {
            for (int i = 0; i < MaxHistoryCount * 2; i++) sensor.AddObservation(0f);
            return;
        }

        // Update history with current position
        Vector2Int currentPos = gridPlaceable.Position;
        if (positionHistory.Count > 0 && positionHistory.Peek() != currentPos)
        {
            // This is a simple way to track movement. 
            // We want the literal history of positions sampled at decision time.
            // If the agent hasn't moved, we might still want to shift the history 
            // or just keep it as is. Usually, for temporal dependencies, 
            // we want the history of N fixed-interval samples.
        }

        // To keep it simple and effective: 
        // Always push current pos and pop oldest if over limit.
        // But only if it's a new position? No, let's just keep the last 5 sampled positions.
        positionHistory.Enqueue(currentPos);
        if (positionHistory.Count > MaxHistoryCount)
        {
            positionHistory.Dequeue();
        }

        Vector2Int gridSize = gridPlaceable.CurrentGrid.Size;

        // Add history (current + 4 previous) to observations
        // We'll peek through the queue.
        foreach (var pos in positionHistory)
        {
            // Normalize observations for better model performance
            sensor.AddObservation((float)pos.x / gridSize.x);
            sensor.AddObservation((float)pos.y / gridSize.y);
        }
    }

    // ---- ML-Agents overrides ----

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int moveAction = actionBuffers.DiscreteActions[0];

        Vector2Int direction = Directions[moveAction];

        if (direction != Vector2Int.zero)
        {
            gridPlaceable.Move(direction);
        }
        AddReward(-0.002f); // small step penalty to encourage efficiency
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = currentHeuristicAction;
    }

    // ---- IMoveInputHandler ----

    public void Move(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 inputDirection = context.ReadValue<Vector2>();

            if (inputDirection.y > 0.5f) currentHeuristicAction = (int)MoveAction.Up;
            else if (inputDirection.y < -0.5f) currentHeuristicAction = (int)MoveAction.Down;
            else if (inputDirection.x < -0.5f) currentHeuristicAction = (int)MoveAction.Left;
            else if (inputDirection.x > 0.5f) currentHeuristicAction = (int)MoveAction.Right;
            else currentHeuristicAction = (int)MoveAction.NoAction;
        }
        else if (context.canceled)
        {
            currentHeuristicAction = (int)MoveAction.NoAction;
        }
    }
}
