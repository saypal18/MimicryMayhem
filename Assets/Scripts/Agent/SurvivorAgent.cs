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

    // // ---- action history (one-hot encoded, n-1 format) ----
    // // Each slot stores a MoveAction int (0=NoAction, 1=Up, 2=Down, 3=Left, 4=Right).
    // // Encoded as 4 floats: [Up, Down, Left, Right] — NoAction = (0,0,0,0).
    // private readonly Queue<int> actionHistory = new Queue<int>();
    // [SerializeField] private int MaxActionHistory = 16; // number of previous actions to remember

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

        // ResetActionHistory();
    }

    // private void ResetActionHistory()
    // {
    //     actionHistory.Clear();
    //     for (int i = 0; i < MaxActionHistory; i++)
    //     {
    //         actionHistory.Enqueue((int)MoveAction.NoAction);
    //     }
    // }

    // public override void OnEpisodeBegin()
    // {
    //     ResetActionHistory();
    // }

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
            AddReward(2f);
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

    //public override void CollectObservations(VectorSensor sensor)
    //{
    //    // Emit 16 one-hot encoded previous actions.
    //    // Each action occupies 4 floats: [Up, Down, Left, Right]
    //    // NoAction = (0, 0, 0, 0)
    //    foreach (int action in actionHistory)
    //    {
    //        sensor.AddObservation(action == (int)MoveAction.Up ? 1f : 0f);
    //        sensor.AddObservation(action == (int)MoveAction.Down ? 1f : 0f);
    //        sensor.AddObservation(action == (int)MoveAction.Left ? 1f : 0f);
    //        sensor.AddObservation(action == (int)MoveAction.Right ? 1f : 0f);
    //    }
    //}

    // ---- ML-Agents overrides ----

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int moveAction = actionBuffers.DiscreteActions[0];

        // // Record this action in history before executing it
        // actionHistory.Enqueue(moveAction);
        // if (actionHistory.Count > MaxActionHistory)
        // {
        //     actionHistory.Dequeue();
        // }

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
