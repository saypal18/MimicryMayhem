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
    private int lastHeuristicMoveAction = (int)MoveAction.Right; // Default direction
    private Vector2Int previousDirection = Vector2Int.zero;
    private bool useAttack = false;
    private Vector2 mousePosition;

    private AbilityController controller;
    private ActiveAbility activeAbility;
    [SerializeField] private CustomGridSensorComponent customGridSensorComponent;
    // ---- direction helpers ----
    private static readonly Vector2Int[] Directions = new[]
    {
        Vector2Int.zero,  // NoAction
        Vector2Int.up,    // Up
        Vector2Int.down,  // Down
        Vector2Int.left,  // Left
        Vector2Int.right  // Right
    };
    private GridPlaceable gridPlaceable;
    private IAbility moveAbility;
    private EquippedItemObservation equippedItemObservation;
    ////

    /// <summary>
    /// Called by Entity.Initialize() to wire up all dependencies.
    /// </summary>
    public void Initialize(
        ITick tick,
        AbilityController controller,
        ActiveAbility ability,
        IAbility moveAbility,

        UnifiedDamageResolver damageResolver,
        DamageDealer damageDealer,
        GridPlaceable gridPlaceable,
        Grid grid,
        EquippedItem equippedItem
        )
    {
        this.controller = controller;
        tick.OnTick += ActOnCooldown;
        this.activeAbility = ability;
        this.moveAbility = moveAbility;
        this.gridPlaceable = gridPlaceable;
        damageResolver.OnDamageTaken += HandleDamageTaken;
        damageDealer.OnDamageDealt += HandleDamageDealt;
        customGridSensorComponent.SetAgentReferences(gridPlaceable, damageDealer, grid);
        this.equippedItemObservation = new EquippedItemObservation(equippedItem);
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
        int action = actionBuffers.DiscreteActions[0];
        if (action == (int)MoveAction.NoAction) return;

        Vector2Int direction = Vector2Int.zero;
        bool isAttack = false;

        // Action space:
        // 0: NoAction
        // 1-4: Move (Up, Down, Left, Right)
        // 5-8: Attack (Up, Down, Left, Right)
        if (action >= 1 && action <= 4)
        {
            direction = Directions[action];
            isAttack = false;
        }
        else if (action >= 5 && action <= 8)
        {
            direction = Directions[action - 4];
            isAttack = true;
        }

        if (direction != Vector2Int.zero)
        {
            previousDirection = direction;
        }
        IAbility abilityToUse;
        if (isAttack)
        {
            activeAbility.UpdateActiveAbility();
            abilityToUse = activeAbility.ability;
        }
        else
        {
            abilityToUse = moveAbility;
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
        if (useAttack)
        {
            // Calculate direction from player to mouse
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z));
            Vector3 directionVector = mouseWorldPos - transform.position;

            // Find cardinal direction with smallest angle
            if (Mathf.Abs(directionVector.x) > Mathf.Abs(directionVector.y))
            {
                if (directionVector.x > 0) discreteActions[0] = 8; // Attack Right
                else discreteActions[0] = 7; // Attack Left
            }
            else
            {
                if (directionVector.y > 0) discreteActions[0] = 5; // Attack Up
                else discreteActions[0] = 6; // Attack Down
            }
        }
        else
        {
            // Move in the currently held WASD direction
            discreteActions[0] = currentHeuristicAction;
        }
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

            if (currentHeuristicAction != (int)MoveAction.NoAction)
            {
                lastHeuristicMoveAction = currentHeuristicAction;
            }
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

    public void OnMouseMove(Vector2 mousePosition)
    {
        this.mousePosition = mousePosition;
    }

    private void HandleDamageTaken()
    {
        // AddReward(-1);
    }

    private void HandleDamageDealt()
    {
        AddReward(1);
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

    // ---- action masking ----

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        if (gridPlaceable == null) return;

        Grid grid = gridPlaceable.CurrentGrid;
        if (grid == null) return;

        Vector2Int pos = gridPlaceable.Position;

        // Action indices: 0=NoAction, 1-4=Move (U,D,L,R), 5-8=Attack (U,D,L,R)
        for (int i = 1; i <= 4; i++)
        {
            Vector2Int direction = Directions[i];
            Vector2Int neighbor = pos + direction;

            bool hasWall = false;
            bool hasEnemy = false;

            var tile = grid.GetTile(neighbor);
            if (tile == null)
            {
                // Out of bounds is treated as a wall
                hasWall = true;
            }
            else
            {
                foreach (var p in tile)
                {
                    if (p.Type == GridPlaceable.PlaceableType.Wall) hasWall = true;
                    if (p.Type == GridPlaceable.PlaceableType.Entity && p != gridPlaceable) hasEnemy = true;
                }
            }

            // Block move if there's a wall or an entity (can't walk into them)
            if (hasWall || hasEnemy)
            {
                actionMask.SetActionEnabled(0, i, false);
            }

            // Block attack if there's a wall (can't attack through walls) or if there's NO enemy
            // (Requirement: if there is an enemy... only then ability use allowed)
            if (hasWall || !hasEnemy)
            {
                actionMask.SetActionEnabled(0, i + 4, false);
            }
        }
    }

    //// ---- decision loop ----

    //private void Update()
    //{
    //    // Request a new decision only when the movement cooldown has elapsed.
    //    if (gridPlaceable != null && gridPlaceable.CanMove())
    //    {
    //        RequestDecision();
    //    }
    //}

    public override void CollectObservations(VectorSensor sensor)
    {
        equippedItemObservation?.CollectObservations(sensor);
    }

    //// ---- ML-Agents overrides ----

    //private void AddBushReward()
    //{
    //    if (gridPlaceable.IsStandingOn(GridPlaceable.PlaceableType.Bush))
    //    {
    //        AddReward(rewardSettings.bushReward);
    //    }
    //}
}
