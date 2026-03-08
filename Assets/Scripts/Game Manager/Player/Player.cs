using UnityEngine;
using System.Collections.Generic;
using Unity.MLAgents.Policies;

public class Player : MonoBehaviour
{

    private EntitySpawner entitySpawner;
    private Grid grid;
    private GameInitializer gameInitializer;
    [SerializeField] private PlayerUI playerUI;
    [SerializeField] private InputManager inputManager;
    private Entity player;

    void UpdatePlayerPower()
    {
        int aliveCount = entitySpawner.ActiveEntityCount;
        int playerPower = player.damageResolver.power;

        playerUI.UpdateStats(aliveCount, playerPower);
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
            }
            else
            {
                bp.BehaviorType = BehaviorType.InferenceOnly;
            }
        }
    }

    private void Start()
    {
        R
    }


}