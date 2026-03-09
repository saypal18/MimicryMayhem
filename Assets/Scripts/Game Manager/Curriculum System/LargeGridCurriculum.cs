// using UnityEngine;
// using Unity.MLAgents;

// [System.Serializable]
// public class LargeGridCurriculum : ICurriculum
// {
//     private Grid grid;
//     private EntitySpawner entitySpawner;

//     public void Initialize(GameInitializer initializer)
//     {
//         grid = initializer.grid;
//         entitySpawner = initializer.entitySpawner;
//     }

//     public void UpdateCurriculumParameters()
//     {
//         var envParams = Academy.Instance.EnvironmentParameters;

//         grid.minGridSize = (int)envParams.GetWithDefault("min_grid_size", grid.minGridSize);
//         grid.maxGridSize = (int)envParams.GetWithDefault("max_grid_size", grid.maxGridSize);

//         entitySpawner.entityPercentage = envParams.GetWithDefault("entity_percentage", entitySpawner.entityPercentage);
//     }
// }