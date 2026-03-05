using Unity.MLAgents;
using UnityEngine;
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem;
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
    private IGridPlaceable gridPlaceable;
    private int currentHeuristicAction = 0;

    public override void Initialize()
    {
        gridPlaceable = GetComponent<IGridPlaceable>();
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int moveAction = actionBuffers.DiscreteActions[0];
        Vector2Int direction = Vector2Int.zero;

        // 0: None, 1: Up, 2: Down, 3: Left, 4: Right
        switch (moveAction)
        {
            case (int)MoveAction.NoAction: direction = Vector2Int.zero; break;
            case (int)MoveAction.Up: direction = Vector2Int.up; break;
            case (int)MoveAction.Down: direction = Vector2Int.down; break;
            case (int)MoveAction.Left: direction = Vector2Int.left; break;
            case (int)MoveAction.Right: direction = Vector2Int.right; break;
        }

        if (direction != Vector2Int.zero)
        {
            gridPlaceable.Move(direction);
            // Debug.Log("Moving");
            AddReward(-0.002f);
        }
        // Debug.Log("Still");


        // Small negative reward each step to encourage fast completion
        // AddReward(-0.001f);
    }

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
            currentHeuristicAction = 0;
        }
    }
}