using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] private GridPlaceable gridPlaceable;
    [SerializeField] private SurvivorAgent survivorAgent;
    [SerializeField] private CollisionResolver collisionResolver;
    [SerializeField] private PickupHandler pickupHandler;
    [SerializeField] public DamageResolver damageResolver;
    [SerializeField] private SizeHandler sizeHandler;
    private IMovement movement;
    public void Initialize(Grid grid, Vector2Int startPosition, MovementFactory movementFactory, EntitySpawner entitySpawner)
    {
        movement = movementFactory.GetMovement(this);
        gridPlaceable.Initialize(grid, startPosition, movement);

        collisionResolver.Initialize();
        pickupHandler.Initialize(collisionResolver);
        damageResolver.Initialize(collisionResolver, pickupHandler);
        sizeHandler.Initialize(transform, pickupHandler);

        survivorAgent.Initialize(gridPlaceable, damageResolver, pickupHandler, entitySpawner, this);
    }
}
