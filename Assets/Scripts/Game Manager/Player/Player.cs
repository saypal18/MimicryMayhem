using UnityEngine;
using System.Collections.Generic;
using Unity.MLAgents.Policies;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlayerUI playerUI;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private Image cooldownImage;

    [Header("Multi-Grid Play Mode")]
    [SerializeField] private List<GameInitializer> environments = new List<GameInitializer>();
    
    private Entity player;
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
        // Unbind previous callbacks to prevent multiple triggers
        foreach (var env in environments)
        {
            env.onEnvironmentReset -= CreatePvEScenario;
            env.entitySpawner.colorize = false;
        }

        player = null;

        // Bind creation logic to the first grid ONLY
        environments[0].onEnvironmentReset += CreatePvEScenario;

        // Reset all
        foreach (var env in environments)
        {
            env.ResetEnvironment();
        }

        environments[0].onEnvironmentReset -= CreatePvEScenario;
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
                    bp.BehaviorType = BehaviorType.HeuristicOnly;
                    if (activeEntities[i].TryGetComponent(out IMoveInputHandler handler))
                        inputManager.InitializeMove(handler);
                    inputManager.InitializeScroll(player.equippedItem);
                    inputManager.InitializeClickMap(env.grid, player.playerActionHighlighter);
                    inputManager.agentTransform = player.transform;
                    player.abilityController.cooldownImage = cooldownImage;
                    if (player.playerActionHighlighter != null)
                    {
                        player.playerActionHighlighter.enabled = true;
                    }
                    inventoryUI.AssignInventory(player.inventory);
                    inventoryUI.Assign(player);
                    inventoryUI.AssignEquippedItem(player.equippedItem);
                }
                else
                {
                    bp.BehaviorType = BehaviorType.InferenceOnly;
                }
            }
            env.MaxSteps = 0; // Disable time-based reset for player control mode across all grids
        }
        
        points = 0;
        mainCamera.transform.parent = player.transform;
        mainCamera.transform.localPosition = new Vector3(0, 0, -10);
    }

    private void StartEnvironment()
    {
        if (environments == null || environments.Count == 0)
        {
            Debug.LogWarning("Player: No environments assigned in the Inspector!");
            return;
        }

        playerUI.gridSizeSlider.onValueChanged.AddListener((value) => {
            foreach (var env in environments) env.grid.SetSize(new Vector2Int((int)value, (int)value));
        });
        playerUI.gridSizeSlider.onValueChanged.Invoke(playerUI.gridSizeSlider.value); 

        playerUI.enemyCountSlider.onValueChanged.AddListener((value) => {
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
        StartEnvironment();
    }

    private void Update()
    {
        if (teleporter != null)
        {
            teleporter.TeleportIfOnDoor(player);
        }
    }

}
