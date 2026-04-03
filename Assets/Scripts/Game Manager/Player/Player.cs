using UnityEngine;
using System.Collections.Generic;
using Unity.MLAgents.Policies;
using UnityEngine.UI;
using Unity.Cinemachine;

public class Player : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlayerUI playerUI;
    [SerializeField] private CinemachineTargetGroup targetGroup;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private Image cooldownImage;
    [SerializeField] private MouseFollower mouseFollower;
    [SerializeField] private Transform highlightParent;
    [SerializeField] private QuestManager questManager;
    [SerializeField] private ForceTurnButton forceTurnButton;
    [SerializeField] private int playerInventorySize = 8;
    [SerializeField] private bool useCustomInventorySize = false;
    [SerializeField] private GameObject playerSpotlightPrefab;
    [SerializeField] private VictoryAnimationController victoryAnimationController;

    [Header("Fade Out UI")]
    [SerializeField] private Image fadeOutImage;
    [SerializeField] private float fadeOutDuration = 2f;
    [SerializeField] private float fadeOutDelay = 2f;


    [Header("Multi-Grid Play Mode")]
    [SerializeField] private List<GameInitializer> environments = new List<GameInitializer>();

    private Entity player;
    public System.Action<Entity> OnPlayerSpawned;
    private int points;

    private GameInitializer GetPlayerEnvironment()
    {
        if (player == null && environments.Count > 0) return environments[0];
        foreach (var env in environments)
        {
            if (env.grid == player?.CurrentGrid) return env;
        }
        return environments.Count > 0 ? environments[0] : null;
    }

    private PlayerTeleporter teleporter;

    private void ResetAllEnvironments()
    {
        if (questManager != null) questManager.ClearAll();

        // Unbind previous callbacks to prevent multiple triggers
        foreach (var env in environments)
        {
            // env.onEnvironmentReset -= CreatePvEScenario;
            env.MaxSteps = 0; // Disable time-based reset for player control mode across all grids
            env.entitySpawner.colorize = false;
        }

        player = null;

        // Bind creation logic to the first grid ONLY
        // environments[0].onEnvironmentReset += CreatePvEScenario;

        // Reset all
        foreach (var env in environments)
        {
            env.ResetEnvironment();
            env.MaxSteps = 0; // Disable time-based reset for player control mode across all grids
        }

        CreatePvEScenario();
        // environments[0].onEnvironmentReset -= CreatePvEScenario;
    }

    void CreatePvEScenario()
    {
        foreach (var env in environments)
        {
            IReadOnlyList<Entity> activeEntities = env.entitySpawner.GetActiveEntities();
            for (int i = 0; i < activeEntities.Count; i++)
            {
                if (!activeEntities[i].TryGetComponent(out BehaviorParameters bp)) continue;

                if (env == environments[0] && i == 0)
                {
                    player = activeEntities[i];

                    if (useCustomInventorySize && player.inventory != null)
                    {
                        player.inventory.slotCount = playerInventorySize;
                        player.inventory.Initialize();
                    }


                    // // If team reservation is enabled, move the player to the reserved team
                    // if (env.entitySpawner.reserveTeamForPlayer)
                    // {
                    //     int reservedId = env.entitySpawner.reservedTeamId;
                    //     if (player.TeamId != reservedId)
                    //     {
                    //         env.entitySpawner.RemoveEntitySafely(player);
                    //         player.TeamId = reservedId;
                    //         env.entitySpawner.AddEntitySafely(player);

                    //         // Update agent and highlighter ticks for the new team
                    //         ITick newTick = env.turnManager.GetTeams()[reservedId];
                    //         player.agent.UpdateTick(newTick);
                    //         if (player.playerActionHighlighter != null)
                    //         {
                    //             player.playerActionHighlighter.UpdateEnvironment(env.grid, newTick);
                    //         }
                    //     }
                    // }

                    bp.BehaviorType = BehaviorType.HeuristicOnly;
                    player.agent.isRuleBased = false; // Human player is never rule-based
                    player.entitySpawner.SyncAnimation(player);
                    if (activeEntities[i].TryGetComponent(out IMoveInputHandler handler))
                        inputManager.InitializeMove(handler);
                    inputManager.InitializeScroll(player.equippedItem);
                    inputManager.InitializeClickMap(env.grid, player.playerActionHighlighter, mainCamera);
                    inputManager.agentTransform = player.transform;
                    player.abilityController.cooldownImage = cooldownImage;
                    if (player.playerActionHighlighter != null)
                    {
                        player.playerActionHighlighter.highlightParent = highlightParent;
                        player.playerActionHighlighter.enabled = true;
                        
                        if (victoryAnimationController != null)
                            victoryAnimationController.SetPlayerActionHighlighter(player.playerActionHighlighter);
                    }
                    inventoryUI.AssignInventory(player.inventory);
                    inventoryUI.Assign(player);
                    inventoryUI.AssignEquippedItem(player.equippedItem);

                    if (mouseFollower != null)
                        mouseFollower.Initialize(inputManager, player.transform);

                    OnPlayerSpawned?.Invoke(player);
                    if (forceTurnButton != null)
                        forceTurnButton.Initialize(env.turnManager);

                    if (playerSpotlightPrefab != null)
                    {
                        playerSpotlightPrefab.transform.SetParent(player.transform);
                        playerSpotlightPrefab.transform.localPosition = Vector3.zero;
                    }
                }
                else
                {
                    bool isAgentRuleBased = (env.entitySpawner.agentType == GameInitializer.AgentType.Randomized)
                        ? activeEntities[i].agent.isRuleBased
                        : (env.entitySpawner.agentType == GameInitializer.AgentType.RuleBased);

                    if (isAgentRuleBased)
                    {
                        bp.BehaviorType = BehaviorType.HeuristicOnly;
                    }
                    else
                    {
                        bp.BehaviorType = BehaviorType.InferenceOnly;
                    }
                }
            }
        }

        points = 0;
        if (targetGroup != null && targetGroup.Targets.Count > 0)
        {
            targetGroup.Targets[0] = new CinemachineTargetGroup.Target
            {
                Object = player.transform,
                Weight = targetGroup.Targets[0].Weight,
                Radius = targetGroup.Targets[0].Radius
            };
        }
    }

    private void StartEnvironment()
    {
        if (environments == null || environments.Count == 0)
        {
            Debug.LogWarning("Player: No environments assigned in the Inspector!");
            return;
        }

        playerUI.gridSizeSlider.onValueChanged.AddListener((value) =>
        {
            foreach (var env in environments) env.grid.SetSize(new Vector2Int((int)value, (int)value));
        });
        playerUI.gridSizeSlider.onValueChanged.Invoke(playerUI.gridSizeSlider.value);

        playerUI.enemyCountSlider.onValueChanged.AddListener((value) =>
        {
            foreach (var env in environments) env.entitySpawner.SetEntityCount((int)value);
        });
        playerUI.enemyCountSlider.onValueChanged.Invoke(playerUI.enemyCountSlider.value);

        playerUI.randomizeToggle.onValueChanged.AddListener(SetGridRandomization);
        playerUI.randomizeToggle.onValueChanged.Invoke(playerUI.randomizeToggle.isOn);

        playerUI.restartButton.onClick.AddListener(ResetAllEnvironments);


        ResetAllEnvironments();
    }

    private void SetGridRandomization(bool enabled)
    {
        playerUI.gridSizeSlider.interactable = !enabled;
        playerUI.enemyCountSlider.interactable = !enabled;
        foreach (var env in environments) env.shouldRandomize = enabled;
    }

    private void Start()
    {
        teleporter = gameObject.AddComponent<PlayerTeleporter>();
        teleporter.inputManager = inputManager;
        teleporter.cam = mainCamera;
        teleporter.OnTeleported += (newEnv) =>
        {
            if (forceTurnButton != null)
                forceTurnButton.Initialize(newEnv.turnManager);
        };
        if (fadeOutImage != null)
        {
            StartCoroutine(FadeOut());
        }

        StartEnvironment();
    }

    private System.Collections.IEnumerator FadeOut()
    {
        if (fadeOutDelay > 0)
            yield return new WaitForSeconds(fadeOutDelay);

        float startAlpha = fadeOutImage.color.a;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            Color c = fadeOutImage.color;
            c.a = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
            fadeOutImage.color = c;
            yield return null;
        }

        Color finalColor = fadeOutImage.color;
        finalColor.a = 0f;
        fadeOutImage.color = finalColor;
        fadeOutImage.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (teleporter != null && player != null && player.moveInfo != null && !player.moveInfo.IsMoving)
        {
            teleporter.TeleportIfOnDoor(player, player.transform.position);
        }
    }

}
