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
public class AttackerAgent : Agent, IMoveInputHandler
{

    // ---- heuristic state ----


    ////
    private int currentHeuristicAction = 0;
    private Vector2Int previousDirection = Vector2Int.zero;
    private bool useAttack = false;

    private AbilityController controller;
    private ActiveAbility activeAbility;
    // ---- direction helpers ----
    private static readonly Vector2Int[] Directions = new[]
    {
        Vector2Int.zero,  // NoAction
        Vector2Int.up,    // Up
        Vector2Int.down,  // Down
        Vector2Int.left,  // Left
        Vector2Int.right  // Right
    };
    IAbility moveAbility;
    ////

    /// <summary>
    /// Called by Entity.Initialize() to wire up all dependencies.
    /// </summary>
    public void Initialize(
        ITick tick,
        AbilityController controller,
        ActiveAbility ability,
        IAbility moveAbility


        )
    {
        this.controller = controller;
        tick.OnTick += ActOnCooldown;
        this.activeAbility = ability;
        this.moveAbility = moveAbility;

    }

    private void ActOnCooldown()
    {
        if (controller.CanAct())
        {
            RequestDecision();
        }
    }
    // whatever direction is received save it unless no input direction. This will be used for attack
    // next if attack action is received and there is a previous direction, perform the attack in that direction. If there is no previous direction, do not perform the attack.
    // if no attack action then just move in the direction received if there is one. If there is no direction received, do not move.
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int moveAction = actionBuffers.DiscreteActions[0];
        Vector2Int direction = Directions[moveAction];
        int attackAction = actionBuffers.DiscreteActions[1];
        if (direction != Vector2Int.zero)
        {
            previousDirection = direction;
        }
        IAbility abilityToUse = null;
        if (attackAction == 0 && direction != Vector2Int.zero)
        {
            abilityToUse = moveAbility;
        }
        else if (previousDirection != Vector2Int.zero && attackAction == 1)
        {
            abilityToUse = activeAbility.ability;
        }
        if (abilityToUse != null)
        {
            abilityToUse.SetDirection(previousDirection);
            controller.Act(abilityToUse);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = currentHeuristicAction;
        discreteActions[1] = useAttack ? 1 : 0;
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

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            useAttack = true;
        }
        else if (context.canceled)
        {
            useAttack = false;
        }
    }
    ////

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

    //private void HandleDamageTaken()
    //{
    //    AddReward(rewardSettings.deathReward);
    //    EndEpisode();
    //    // Trigger the full despawn chain: OnDespawning → GridPlaceable.RemoveFromGrid
    //    // → EntitySpawner.activeEntities auto-remove via CreateDespawnHandler
    //    PoolingEntity.Despawn(gameObject);
    //}

    //private void HandleDamageDealt()
    //{
    //    AddReward(rewardSettings.damageReward);

    //    // Check win condition: are we the last entity on the grid?
    //    if (entitySpawner != null && entitySpawner.IsLastEntity(thisEntity))
    //    {
    //        AddReward(rewardSettings.damageReward);
    //        EndEpisode();
    //        if (gameInitializer != null)
    //        {
    //            gameInitializer.ResetEnvironment();
    //        }
    //    }
    //}

    //private void HandlePickupCollected(Pickup pickup)
    //{
    //    AddReward(rewardSettings.pickupReward);
    //}

    //// ---- action masking ----

    //public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    //{
    //    if (gridPlaceable == null) return;

    //    Grid grid = gridPlaceable.CurrentGrid;
    //    if (grid == null) return;

    //    Vector2Int pos = gridPlaceable.Position;

    //    // MoveAction indices: 0=NoAction,1=Up,2=Down,3=Left,4=Right
    //    for (int action = 1; action <= 4; action++)
    //    {
    //        Vector2Int neighbor = pos + Directions[action];
    //        if (!grid.IsMovable(neighbor))
    //        {
    //            actionMask.SetActionEnabled(0, action, false);
    //        }
    //    }
    //}

    //// ---- decision loop ----

    //private void Update()
    //{
    //    // Request a new decision only when the movement cooldown has elapsed.
    //    if (gridPlaceable != null && gridPlaceable.CanMove())
    //    {
    //        RequestDecision();
    //    }
    //}

    ////public override void CollectObservations(VectorSensor sensor)
    ////{
    ////    // Emit 16 one-hot encoded previous actions.
    ////    // Each action occupies 4 floats: [Up, Down, Left, Right]
    ////    // NoAction = (0, 0, 0, 0)
    ////    foreach (int action in actionHistory)
    ////    {
    ////        sensor.AddObservation(action == (int)MoveAction.Up ? 1f : 0f);
    ////        sensor.AddObservation(action == (int)MoveAction.Down ? 1f : 0f);
    ////        sensor.AddObservation(action == (int)MoveAction.Left ? 1f : 0f);
    ////        sensor.AddObservation(action == (int)MoveAction.Right ? 1f : 0f);
    ////    }
    ////}

    //// ---- ML-Agents overrides ----

    //private void AddBushReward()
    //{
    //    if (gridPlaceable.IsStandingOn(GridPlaceable.PlaceableType.Bush))
    //    {
    //        AddReward(rewardSettings.bushReward);
    //    }
    //}
}
