using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using UnityEngine.InputSystem;
public class PickupCollector : Agent, IMoveInputHandler
{
    private Grid grid;
    private PickupPlacer pickupPlacer;
    private GridMovement gridMovement;
    private PickupHandler pickupHandler;
    private int currentHeuristicAction = 0;
    // ... (omitting lines for brevity in instruction, will write full block below)

    public void Initialize(Grid grid, PickupPlacer pickupPlacer, GridMovement gridMovement, PickupHandler pickupHandler)
    {
        this.grid = grid;
        this.pickupPlacer = pickupPlacer;
        this.gridMovement = gridMovement;
        this.pickupHandler = pickupHandler;
    }

    private void FixedUpdate()
    {
        if (gridMovement != null && gridMovement.IsMovable)
        {
            RequestDecision();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (grid == null || pickupPlacer == null) return;

        Vector2Int currentPos = grid.GetGridPosition(transform.position);
        sensor.AddObservation(currentPos.x);
        sensor.AddObservation(currentPos.y);

        List<Vector2Int> pickups = pickupPlacer.GetPickupPositions();
        // Sort by distance to the agent to provide the most relevant info first
        pickups.Sort((a, b) =>
            Vector2.Distance(currentPos, a).CompareTo(Vector2.Distance(currentPos, b))
        );

        const int maxPickupsToShow = 5;
        for (int i = 0; i < maxPickupsToShow; i++)
        {
            if (i < pickups.Count)
            {
                sensor.AddObservation(pickups[i].x);
                sensor.AddObservation(pickups[i].y);
            }
            else
            {
                // Sentinel value for "no pickup"
                sensor.AddObservation(-1);
                sensor.AddObservation(-1);
            }
        }
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int moveAction = actionBuffers.DiscreteActions[0];
        Vector2Int direction = Vector2Int.zero;

        // 0: None, 1: Up, 2: Down, 3: Left, 4: Right
        switch (moveAction)
        {
            case 1: direction = Vector2Int.up; break;
            case 2: direction = Vector2Int.down; break;
            case 3: direction = Vector2Int.left; break;
            case 4: direction = Vector2Int.right; break;
            case 5: direction = Vector2Int.zero; break;
        }

        if (direction != Vector2Int.zero)
        {
            gridMovement.Move(direction);
        }

        // Small negative reward each step to encourage fast completion
        AddReward(-0.001f);
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 inputDirection = context.ReadValue<Vector2>();

            if (inputDirection.y > 0.5f) currentHeuristicAction = 1;
            else if (inputDirection.y < -0.5f) currentHeuristicAction = 2;
            else if (inputDirection.x < -0.5f) currentHeuristicAction = 3;
            else if (inputDirection.x > 0.5f) currentHeuristicAction = 4;
            else currentHeuristicAction = 0;
        }
        else if (context.canceled)
        {
            currentHeuristicAction = 0;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = currentHeuristicAction;
    }

    public void OnPickupCollected()
    {
        AddReward(1.0f);
        if (pickupPlacer.GetPickupPositions().Count == 0)
        {
            gridMovement.SetPosition(Vector2Int.zero);
            pickupPlacer.RandomPlacement(5);
            pickupHandler.Reset();
            EndEpisode();
        }
    }
}