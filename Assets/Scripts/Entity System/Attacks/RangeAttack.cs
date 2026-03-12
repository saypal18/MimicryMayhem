//using UnityEngine;

//public class RangeAttack : MonoBehaviour, IAbility
//{
//    [SerializeField] private Transform startPosition;
//    [SerializeField] private Projectile projectilePrefab;
//    [SerializeField] private float projectileSpeed;
//    [SerializeField] private int maxDistance;  // number of tiles to move

//    private Vector2Int currentDirection;

//    public void SetDirection(Vector2Int direction)
//    {
//        currentDirection = direction;
//    }

//    public bool Perform()
//    {
//        if (currentDirection == Vector2Int.zero) return false;

//        Vector3 worldDirection = new Vector3(currentDirection.x, 0, currentDirection.y);
//        Projectile projectile = PoolingEntity.Spawn(projectilePrefab, startPosition.position, Quaternion.identity);
//        projectile.Go(worldDirection, projectileSpeed, maxDistance);
//        return true;
//    }
//}