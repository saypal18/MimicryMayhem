using UnityEngine;
using Unity.MLAgents;

[System.Serializable]
public class BasicBushCurriculum : ICurriculum
{
    private PerlinBushConfig bushConfig;
    private RewardSettings rewardSettings;
    private Grid grid;
    private EntitySpawner entitySpawner;

    public void Initialize(GameInitializer initializer)
    {
        bushConfig = initializer.bushPlacer.perlinConfig;
        rewardSettings = initializer.entitySpawner.rewardSettings;
        grid = initializer.grid;
        entitySpawner = initializer.entitySpawner;
    }

    public void UpdateCurriculumParameters()
    {
        var envParams = Academy.Instance.EnvironmentParameters;

        // // Bush density curriculum
        // bushConfig.threshold = 1 - envParams.GetWithDefault("bush_density", 0.3f);

        // Reward curriculum
        rewardSettings.pickupReward = envParams.GetWithDefault("pickup_reward", 0f);
        rewardSettings.stepReward = envParams.GetWithDefault("step_reward", 0f);
        rewardSettings.bushReward = envParams.GetWithDefault("bush_reward", 0f);
        // rewardSettings.deathReward = envParams.GetWithDefault("death_reward", -0.5f);

        // Grid curriculum
        grid.minGridSize = (int)envParams.GetWithDefault("min_grid_size", grid.minGridSize);
        grid.maxGridSize = (int)envParams.GetWithDefault("max_grid_size", grid.maxGridSize);

        // Entity curriculum
        entitySpawner.entityPercentage = envParams.GetWithDefault("entity_percentage", entitySpawner.entityPercentage);
        entitySpawner.teamsEnabled = envParams.GetWithDefault("teams", entitySpawner.teamsEnabled ? 1f : 0f) > 0.5f;
    }
}