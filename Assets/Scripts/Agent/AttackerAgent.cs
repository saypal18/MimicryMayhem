using Unity.MLAgents;
using UnityEngine;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
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
    [SerializeField] private BehaviorParameters behaviorParameters;
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
    private ITick tick;
    private EquippedItem equippedItem;
    private Entity entity;
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
        EquippedItem equippedItem,
        PickupHandler pickupHandler,
        Entity entity
        )
    {
        this.tick = tick;
        this.controller = controller;
        tick.OnTick += ActOnCooldown;
        this.activeAbility = ability;
        this.moveAbility = moveAbility;
        this.gridPlaceable = gridPlaceable;
        damageResolver.OnDamageTaken += HandleDamageTaken;
        damageDealer.OnDamageDealt += HandleDamageDealt;
        pickupHandler.OnPickupCollected += HandlePickupCollected;
        customGridSensorComponent.SetAgentReferences(gridPlaceable, damageDealer, grid);
        this.equippedItemObservation = new EquippedItemObservation(equippedItem);
        this.equippedItem = equippedItem;
        this.entity = entity;
    }

    private bool pendingDecision = false;

    private void ActOnCooldown()
    {
        if (controller.IsControlled())
        {
            controller.ConsumeControlTurn();
            tick.OnPlayed?.Invoke();
            return;
        }
        pendingDecision = true;
    }

    private void Update()
    {
        if (pendingDecision)
        {
            if (controller.IsControlled())
            {
                controller.ConsumeControlTurn();
                pendingDecision = false;
                tick.OnPlayed?.Invoke();
            }
            else if (controller.CanAct())
            {
                RequestDecision();
            }
        }
    }
    // whatever direction is received save it unless no input direction. This will be used for attack
    // next if attack action is received and there is a previous direction, perform the attack in that direction. If there is no previous direction, do not perform the attack.
    // if no attack action then just move in the direction received if there is one. If there is no direction received, do not move.
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        AddReward(-0.002f);
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
            bool acted = controller.Act(abilityToUse);
            if (acted && isAttack)
            {
                activeAbility.PlayAttackSound(transform.position, entity.IsPlayer);
            }
            pendingDecision = false;
            tick.OnPlayed?.Invoke();
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        int actionIndex = currentHeuristicAction;
        
        if (useAttack && actionIndex != (int)MoveAction.NoAction)
        {
            actionIndex += 4;
        }
        
        discreteActions[0] = actionIndex;
        
        // Reset so action isn't repeated indefinitely
        currentHeuristicAction = (int)MoveAction.NoAction;
        useAttack = false;
    }

    // ---- IMoveInputHandler ----

    public void OnGridClick(Vector2Int gridPosition, bool isAttack)
    {
        if (gridPlaceable == null) return;
        Vector2Int currentPos = gridPlaceable.Position;
        Vector2Int direction = gridPosition - currentPos;

        // Assign move action based on direction (since we normalized length, just check dominancy)
        if (direction.x > 0) currentHeuristicAction = (int)MoveAction.Right;
        else if (direction.x < 0) currentHeuristicAction = (int)MoveAction.Left;
        else if (direction.y > 0) currentHeuristicAction = (int)MoveAction.Up;
        else if (direction.y < 0) currentHeuristicAction = (int)MoveAction.Down;
        else return; // clicked same tile

        useAttack = isAttack;

        // Force decision immediately
        if (controller.CanAct() && pendingDecision)
        {
            RequestDecision();
        }
    }

    public void OnMouseMove(Vector2 mousePosition)
    {
        this.mousePosition = mousePosition;
        if (entity.playerActionHighlighter != null && entity.playerActionHighlighter.enabled)
        {
            entity.playerActionHighlighter.OnMouseMove(mousePosition);
        }
    }

    private void HandleDamageTaken(Entity attacker)
    {
        //AddReward(0);
    }

    private void HandleDamageDealt(Entity victim)
    {
        // AddReward(1f);

        if (victim == null) return;

        if (victim.behaviorParameters != null)
        {
            //Debug.Log("victim team id: " + victim.TeamId + " entity team id: " + entity.TeamId);
            //Debug.Log("behaviour teams: victim :" + victim.behaviorParameters.TeamId + " entity: " + entity.behaviorParameters.TeamId);
            if (victim.TeamId == entity.TeamId)
            {
                AddReward(-1f);
            }
            else
            {
                AddReward(1f);
            }
        }
        else
        {
            // Default to positive reward for hitting non-agent entities (if applicable)
            AddReward(1f);
        }
    }
    private void HandlePickupCollected(Pickup pickup)
    {
        AddReward(0.1f);
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
            if (hasWall)
            {
                actionMask.SetActionEnabled(0, i, false);
            }

            // Block attack if there's a wall (can't attack through walls) or if there's NO enemy
            // (Requirement: if there is an enemy... only then ability use allowed)
            if (hasWall)
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
