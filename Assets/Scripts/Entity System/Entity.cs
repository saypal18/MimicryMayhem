using UnityEngine;
using Unity.MLAgents.Policies;
public class Entity : MonoBehaviour
{
    [SerializeField] private GridPlaceable gridPlaceable;
    [SerializeField] public AttackerAgent agent;
    [SerializeField] public ActiveAbility activeAbility;
    [SerializeField] private MoveAbility moveAbility;
    public AbilityController abilityController;
    // damage resolvers

    [SerializeField] private CollisionResolver collisionResolver;
    [SerializeField] public UnifiedDamageResolver damageResolver;
    [SerializeField] public PickupHandler pickupHandler;
    [SerializeField] public EntityCollisionKnockback entityCollisionKnockback;
    [SerializeField] public DamageDealer damageDealer;
    [SerializeField] public BehaviorParameters behaviorParameters;
    public int TeamId;
    public EquippedItem equippedItem;
    public SortedInventory inventory;
    public MoveInfo moveInfo = new MoveInfo();
    //[SerializeField] private PickupHandler pickupHandler;
    //[SerializeField] private UnifiedDamageResolver _damageResolver;
    //public DamageResolver damageResolver => _damageResolver; // Expose as DamageResolver for easier access, while allowing UnifiedDamageResolver to have its own internal state and logic.
    //[SerializeField] private SizeHandler sizeHandler;
    //[SerializeField] private SpriteRenderer spriteRenderer;
    //public AbilityController action;
    //[SerializeField] public MeleeAttack[] attacks;
    //public IMovement movement { get; private set; }
    public void Initialize(Grid grid, Vector2Int startPosition, EntityMovementFactory movementFactory, ITick tick)
    {
        gridPlaceable.Initialize(grid, startPosition);
        moveAbility.Initialize(movementFactory, gridPlaceable, moveInfo);
        inventory.Initialize();
        equippedItem.Initialize(inventory);
        pickupHandler.Initialize();
        entityCollisionKnockback.Initialize(movementFactory, gridPlaceable, moveInfo, abilityController, inventory);
        collisionResolver.Initialize(pickupHandler, damageResolver, entityCollisionKnockback, abilityController);
        activeAbility.Initialize(grid, damageDealer, equippedItem, inventory, damageDealer, movementFactory, gridPlaceable, moveInfo);
        damageResolver.Initialize(collisionResolver, inventory, equippedItem, movementFactory, abilityController, moveInfo);
        damageDealer.Initialize();
        abilityController.Initialize(moveInfo);
        agent.Initialize(tick, abilityController, activeAbility, moveAbility, damageResolver, damageDealer, gridPlaceable, grid, equippedItem, pickupHandler, this);
    }

    /////// apply during play //////
    void Update()
    {
        abilityController.Update();
    }
}

//public class Entity : MonoBehaviour
//{
//    [SerializeField] private GridPlaceable gridPlaceable;
//    [SerializeField] private SurvivorAgent survivorAgent;
//    [SerializeField] private CollisionResolver collisionResolver;
//    [SerializeField] private PickupHandler pickupHandler;
//    [SerializeField] private UnifiedDamageResolver _damageResolver;
//    public DamageResolver damageResolver => _damageResolver; // Expose as DamageResolver for easier access, while allowing UnifiedDamageResolver to have its own internal state and logic.
//    [SerializeField] private SizeHandler sizeHandler;
//    [SerializeField] private SpriteRenderer spriteRenderer;
//    public AbilityController action;
//    [SerializeField] public MeleeAttack[] attacks;
//    public IMovement movement { get; private set; }
//    public void Initialize(Grid grid, Vector2Int startPosition, MovementFactory movementFactory, EntitySpawner entitySpawner, GameInitializer gameInitializer, RewardSettings rewardSettings)
//    {
//        movement = movementFactory.GetMovement(this);
//        gridPlaceable.Initialize(grid, startPosition, movement);

//        collisionResolver.Initialize();
//        pickupHandler.Initialize(collisionResolver);
//        damageResolver.Initialize(collisionResolver, pickupHandler);
//        sizeHandler.Initialize(transform, pickupHandler);

//        survivorAgent.Initialize(gridPlaceable, damageResolver, pickupHandler, entitySpawner, this, gameInitializer, rewardSettings);
//        //action = new(1);
//        attacks[0].Initialize(grid);
//    }

//    /// <summary>Ends the ML-Agents episode for this entity. Called externally (e.g. on step-limit timeout).</summary>
//    public void ForceEndEpisode()
//    {
//        if (survivorAgent != null)
//        {
//            // survivorAgent.EpisodeInterrupted();
//            survivorAgent.EndEpisode();
//        }
//    }

//    public void SetColor(Color color)
//    {
//        if (spriteRenderer != null)
//        {
//            spriteRenderer.color = color;
//        }
//    }
//}
