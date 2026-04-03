using UnityEngine;
using Unity.MLAgents.Policies;
using System;
using FMODUnity;
using FMOD.Studio;

public class Entity : MonoBehaviour
{
    [SerializeField] public GridPlaceable gridPlaceable;
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
    [SerializeField] public RuleBasedWeaponProvider ruleBasedWeaponProvider;
    [SerializeField] private Animator animator;
    [SerializeField] private Animator movementAnimator;
    [HideInInspector] public EntitySpawner entitySpawner;
    public int TeamId;
    public bool IsPlayer => behaviorParameters != null &&
        behaviorParameters.BehaviorType == BehaviorType.HeuristicOnly &&
        (agent == null || !agent.isRuleBased);
    public bool IsBoss { get; set; } = false;

    public Vector2Int Position => gridPlaceable.Position;
    public Grid CurrentGrid => gridPlaceable.CurrentGrid;
    public bool IsActiveForTurns { get; private set; } = true;
    [SerializeField] public bool IsKillable = true;
    public bool canBeStunned = true;

    public void SetActiveForTurns(bool active)
    {
        if (IsActiveForTurns == active) return;
        IsActiveForTurns = active;
        if (active) PlayActivationBark();
        UpdateBossPresence(active);
        if (agent != null) agent.enabled = active;
    }

    private void UpdateBossPresence(bool active)
    {
        if (Trainer.IsTraining || !IsBoss || SoundManager.Events == null) return;
        if (SoundManager.CheckEventNull(SoundManager.Events.bossPresence, this)) return;

        if (active && !bossPresenceInstance.isValid())
        {
            bossPresenceInstance = RuntimeManager.CreateInstance(SoundManager.Events.bossPresence);
            bossPresenceInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
            bossPresenceInstance.start();
        }
        else if (!active && bossPresenceInstance.isValid())
        {
            bossPresenceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            bossPresenceInstance.release();
        }
    }

    private void ReleaseBossPresence()
    {
        if (bossPresenceInstance.isValid())
        {
            bossPresenceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            bossPresenceInstance.release();
        }
    }

    private void PlayActivationBark()
    {
        if (Trainer.IsTraining || IsPlayer || SoundManager.Events == null) return;

        SoundManager.PlayOneShot(SoundManager.Events.activationBark, transform.position,
            ("CharacterType", IsBoss ? "Boss" : "Enemy"));
    }

    public EquippedItem equippedItem;
    public EquippedItemTierDisplay tierDisplay;
    public SortedInventory inventory;
    public MoveInfo moveInfo = new MoveInfo();
    [SerializeField] public PlayerActionHighlighter playerActionHighlighter;
    [SerializeField] private SpriteRenderer keyVisual;
    [SerializeField] public Transform animationParent;

    private EventInstance bossPresenceInstance;
    public GameObject currentAnimation { get; set; }


    private bool _hasBossKey;
    public bool HasBossKey
    {
        get => _hasBossKey;
        set
        {
            _hasBossKey = value;
            if (keyVisual != null) keyVisual.enabled = value;
        }
    }
    public Action<Entity, WeaponItem, Vector2Int> OnDropItemToGrid;

    public void Initialize(Grid grid, Vector2Int startPosition, EntityMovementFactory movementFactory, ITick tick, EntitySpawner entitySpawner)
    {
        ReleaseBossPresence();
        IsActiveForTurns = true;
        if (agent != null) agent.enabled = true;
        this.entitySpawner = entitySpawner;
        HasBossKey = false;
        IsBoss = false;
        gridPlaceable.Initialize(grid, startPosition);
        moveAbility.Initialize(movementFactory, gridPlaceable, moveInfo);
        inventory.Initialize();

        inventory.OnItemDropped -= HandleItemDropped;
        inventory.OnItemDropped += HandleItemDropped;

        equippedItem.Initialize(inventory);
        pickupHandler.Initialize();
        entityCollisionKnockback.Initialize(movementFactory, gridPlaceable, moveInfo, abilityController, inventory);
        collisionResolver.Initialize(pickupHandler, damageResolver, entityCollisionKnockback, abilityController);
        activeAbility.Initialize(grid, damageDealer, equippedItem, inventory, damageDealer, movementFactory, gridPlaceable, moveInfo, animator);
        damageResolver.Initialize(collisionResolver, inventory, equippedItem, movementFactory, abilityController, moveInfo);
        damageDealer.Initialize();
        abilityController.Initialize(moveInfo);
        agent.Initialize(tick, abilityController, activeAbility, moveAbility, damageResolver, damageDealer, gridPlaceable, grid, equippedItem, pickupHandler, this, entitySpawner);
        if (playerActionHighlighter != null)
        {
            playerActionHighlighter.Initialize(this, grid, equippedItem, tick);
        }
    }

    public void TransferToNewEnvironment(Grid newGrid, Vector2Int newPosition, ITick newTick, EntitySpawner newSpawner)
    {
        SetActiveForTurns(true);

        gridPlaceable.RemoveFromGrid();
        gridPlaceable.Initialize(newGrid, newPosition);

        if (agent != null)
        {
            agent.UpdateGrid(newGrid);
            agent.UpdateTick(newTick);
            agent.UpdateSpawner(newSpawner);
        }

        if (playerActionHighlighter != null)
        {
            playerActionHighlighter.UpdateEnvironment(newGrid, newTick);
        }

        activeAbility.UpdateGrid(newGrid);
    }

    private void HandleItemDropped(WeaponItem item, int slotIndex)
    {
        OnDropItemToGrid?.Invoke(this, item, gridPlaceable.Position);
    }

    void OnDisable()
    {
        ReleaseBossPresence();
    }

    /////// apply during play //////
    void Update()
    {
        if (movementAnimator != null)
        {
            movementAnimator.SetBool("isMoving", moveInfo.IsMoving);
        }
        abilityController.Update();

        if (bossPresenceInstance.isValid())
        {
            bossPresenceInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        }
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
