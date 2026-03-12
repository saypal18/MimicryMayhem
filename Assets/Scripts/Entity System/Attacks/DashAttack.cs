//using UnityEngine;

//public class DashAttack : MonoBehaviour, IAbility
//{
//    [SerializeField] private int dashDistance; // number of tiles to move
//    [SerializeField] private float duration;

//    private Vector2Int currentDirection;
//    // private IMovement movement; // Assuming this will be used for the dash logic soon

//    public void SetDirection(Vector2Int direction)
//    {
//        currentDirection = direction;
//    }

//    public bool Perform()
//    {
//        if (currentDirection == Vector2Int.zero) return false;

//        // Perform dash logic here using currentDirection, dashDistance and duration
//        // e.g., movement.Dash(currentDirection, dashDistance, duration);

//        return true;
//    }
//}