using System.Collections.Generic;
using UnityEngine;

namespace WallSystem
{
    [System.Serializable]
    public class WallPlacer
    {
        [SerializeField] private GridPlaceable wallPrefab;
        [SerializeField] private Transform wallParent;
        private Grid grid;

        public void Initialize(Grid grid)
        {
            this.grid = grid;
        }

        public void SpawnAtPosition(Vector2Int position)
        {
            GridPlaceable wall = PoolingEntity.Spawn(wallPrefab, wallParent);
            wall.Initialize(grid, position);
        }

        public void SpawnAtRandomPositions(int count)
        {
            List<Vector2Int> randomPositions = grid.GetRandomEmptyPositions(count);
            foreach (Vector2Int randomPosition in randomPositions)
            {
                SpawnAtPosition(randomPosition);
            }
        }
    }
}
