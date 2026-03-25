using UnityEngine;
using System.Collections.Generic;
using Unity.MLAgents.Policies;
using UnityEngine.UI;
public class Player : MonoBehaviour
{

    private EntitySpawner entitySpawner;
    private Grid grid;
    [SerializeField] private GameInitializer gameInitializer;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlayerUI playerUI;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private Image cooldownImage;
    private Entity player;
    private int points;
    void UpdatePlayerPower()
    {
        //int aliveCount = entitySpawner.ActiveEntityCount;
        //int playerPower = player.damageResolver.power;

        //playerUI.UpdateStats(aliveCount, playerPower, points);
    }

    // enemy will be 3 distinct shades of red / blue whether 
    // it is more or less powerful and based on their power relative to the player 
    // - lighter = closer to player power. == player power means white
    void UpdateEnemyPowerDisplay()
    {
        //if (entitySpawner == null || player == null || player.damageResolver == null)
        //    return;

        //IReadOnlyList<Entity> activeEntities = entitySpawner.GetActiveEntities();
        //int playerPower = player.damageResolver.power;

        //for (int i = 0; i < activeEntities.Count; i++)
        //{
        //    Entity entity = activeEntities[i];
        //    if (entity == null || entity == player || entity.damageResolver == null)
        //        continue;

        //    int diff = entity.damageResolver.power - playerPower;
        //    Color targetColor;
        //    diff = Mathf.Clamp(diff, -3, 3); // Cap the difference for color tiers
        //    switch(diff)
        //    {
        //        case 3:
        //            targetColor = new Color(0.75f, 0f, 0f); // Strongly stronger - dark red
        //            break;
        //        case 2:
        //            targetColor = new Color(1f, 0.45f, 0.45f); // Moderately stronger - medium red
        //            break;
        //        case 1:
        //            targetColor = new Color(1f, 0.75f, 0.75f); // Slightly stronger - light red
        //            break;
        //        case -1:
        //            targetColor = new Color(0.75f, 0.85f, 1f); // Slightly weaker - light blue
        //            break;
        //        case -2:
        //            targetColor = new Color(0.45f, 0.65f, 1f); // Moderately weaker - medium blue
        //            break;
        //        case -3:
        //            targetColor = new Color(0f, 0.3f, 0.8f); // Strongly weaker - dark blue
        //            break;
        //        default:
        //            targetColor = Color.white; // Equal power
        //            break;
        //    }
        //    entity.SetColor(targetColor);
        //}
    }

    void CreatePvEScenario()
    {
        IReadOnlyList<Entity> activeEntities = entitySpawner.GetActiveEntities();
        for (int i = 0; i < activeEntities.Count; i++)
        {
            if (!activeEntities[i].TryGetComponent(out BehaviorParameters bp)) continue;

            if (i == 0)
            {
                player = activeEntities[i];
                bp.BehaviorType = BehaviorType.HeuristicOnly;
                if (activeEntities[i].TryGetComponent(out IMoveInputHandler handler))
                    inputManager.InitializeMove(handler);
                inputManager.InitializeScroll(player.equippedItem);
                inputManager.InitializeClickMap(grid, player.playerActionHighlighter);
                inputManager.agentTransform = player.transform;
                player.abilityController.cooldownImage = cooldownImage;
                if (player.playerActionHighlighter != null)
                {
                    player.playerActionHighlighter.enabled = true;
                }
                inventoryUI.AssignInventory(player.inventory);
                inventoryUI.AssignEquippedItem(player.equippedItem);
            }
            else
            {
                bp.BehaviorType = BehaviorType.InferenceOnly;
            }
        }
        gameInitializer.MaxSteps = 0; // Disable time-based reset for player control mode
        points = 0;
        //player.damageResolver.OnDamageDealt += () => points++;
        mainCamera.transform.parent = player.transform;
        mainCamera.transform.localPosition = new Vector3(0, 0, -10);
        //mainCamera.transform.localPosition = new Vector3(-5, 0, -10);
        //player.damageResolver.OnDamageTaken += () => mainCamera.transform.parent = null;
        //player.SetColor(Color.white);
    }

    private void StartEnvironment()
    {
        grid = gameInitializer.grid;
        entitySpawner = gameInitializer.entitySpawner;
        playerUI.gridSizeSlider.onValueChanged.AddListener((value) => grid.SetSize(new Vector2Int((int)value, (int)value)));
        playerUI.gridSizeSlider.onValueChanged.Invoke(playerUI.gridSizeSlider.value); // Apply initial value
        playerUI.enemyCountSlider.onValueChanged.AddListener((value) => entitySpawner.SetEntityCount((int)value));
        playerUI.enemyCountSlider.onValueChanged.Invoke(playerUI.enemyCountSlider.value); // Apply initial value
        playerUI.randomizeToggle.onValueChanged.AddListener(SetGridRandomization);
        playerUI.randomizeToggle.onValueChanged.Invoke(playerUI.randomizeToggle.isOn); // Apply initial value

        playerUI.restartButton.onClick.AddListener(() => gameInitializer.ResetEnvironment());
        gameInitializer.onEnvironmentReset += CreatePvEScenario;
        entitySpawner.colorize = false;
        gameInitializer.ResetEnvironment();
    }

    private void SetGridRandomization(bool enabled)
    {
        playerUI.gridSizeSlider.interactable = !enabled;
        playerUI.enemyCountSlider.interactable = !enabled;
        gameInitializer.shouldRandomize = enabled;
    }

    private void Start()
    {
        StartEnvironment();
    }

    //private void Update()
    //{
    //    UpdatePlayerPower();
    //    UpdateEnemyPowerDisplay();
    //}

}
