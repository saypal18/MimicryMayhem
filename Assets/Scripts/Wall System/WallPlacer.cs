using System.Collections.Generic;
using UnityEngine;

namespace WallSystem
{
    [System.Serializable]
    public class WallPlacer
    {
        public enum WallMode { Random, PerlinNoise, RandomEachReset }

        [Header("Wall Settings")]
        [SerializeField] private WallMode wallMode = WallMode.RandomEachReset;
        [Tooltip("When WallMode is RandomEachReset: probability (0–1) that a given reset uses Perlin noise instead of scatter.")]
        [Range(0f, 1f)]
        [SerializeField] private float perlinNoiseProbability = 0.5f;
        [SerializeField] private PerlinWallConfig perlinConfig = new PerlinWallConfig();
        [SerializeField] private float wallPercentage = 10f;

        [Header("References")]
        [SerializeField] private GridPlaceable wallPrefab;
        [SerializeField] private Transform wallParent;
        private Grid grid;

        // Noise map persisted from SpawnPerlinNoise so the connectivity pass
        // can sort/prioritise wall removals by their original noise value.
        private float[,] noiseMap;

        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        public void Initialize(Grid grid)
        {
            this.grid = grid;
        }

        public void SpawnAtPosition(Vector2Int position)
        {
            GridPlaceable wall = PoolingEntity.Spawn(wallPrefab, wallParent);
            wall.Initialize(grid, position);
        }

        // ── Main Placement method ──────────────────────────────────────────────────
        public void PlaceWalls(int totalArea)
        {
            int scatterWallCount = Mathf.RoundToInt(totalArea * (wallPercentage / 100f));
            WallMode activeMode = wallMode;

            if (wallMode == WallMode.RandomEachReset)
                activeMode = (Random.value < perlinNoiseProbability) ? WallMode.PerlinNoise : WallMode.Random;

            if (activeMode == WallMode.PerlinNoise)
                SpawnPerlinNoise(perlinConfig);
            else
                SpawnAtRandomPositions(scatterWallCount);
        }

        // ── Scatter mode (no post-processing) ───────────────────────────────
        public void SpawnAtRandomPositions(int count)
        {
            List<Vector2Int> randomPositions = grid.GetRandomEmptyPositions(count);
            foreach (Vector2Int randomPosition in randomPositions)
                SpawnAtPosition(randomPosition);
        }

        // ── Perlin noise mode ────────────────────────────────────────────────
        public void SpawnPerlinNoise(PerlinWallConfig config)
        {
            float offsetX = Random.Range(0f, 9999f);
            float offsetY = Random.Range(0f, 9999f);
            float scaleX = Random.Range(config.scaleXMin, config.scaleXMax);
            float scaleY = Random.Range(config.scaleYMin, config.scaleYMax);
            float angleRad = Random.Range(config.rotationAngleMin, config.rotationAngleMax) * Mathf.Deg2Rad;
            float cosA = Mathf.Cos(angleRad);
            float sinA = Mathf.Sin(angleRad);

            int sizeX = grid.Size.x;
            int sizeY = grid.Size.y;
            noiseMap = new float[sizeX, sizeY];

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    // Rotate sampling coords to produce diagonal wall stretching.
                    float rx = x * cosA - y * sinA;
                    float ry = x * sinA + y * cosA;
                    float noise = Mathf.PerlinNoise(rx * scaleX + offsetX, ry * scaleY + offsetY);
                    noiseMap[x, y] = noise;

                    if (noise > config.threshold)
                    {
                        Vector2Int pos = new Vector2Int(x, y);
                        if (grid.IsPositionEmpty(pos))
                            SpawnAtPosition(pos);
                    }
                }
            }

            EnforceConnectivityAndCap(config);
        }

        // ── Connectivity + cap enforcement (Perlin mode only) ────────────────
        // Algorithm:
        //   1. Clear the center tile if it's a wall.
        //   2. BFS from center → build floodFill and a frontier of adjacent walls.
        //   3. While (floodFill + wallCount < totalTiles):
        //        a. Early-stop if coverage ≥ minConnectedCoveragePercent AND walls ≤ maxWallPercentEarlyStop.
        //        b. Despawn the frontier wall with the lowest noise value.
        //        c. Expand BFS from that tile to grow floodFill and update frontier.
        //   4. If wallCount > maxWallCapPercent: warn AND despawn from lowest noise.
        private void EnforceConnectivityAndCap(PerlinWallConfig config)
        {
            int sizeX = grid.Size.x;
            int sizeY = grid.Size.y;
            int totalTiles = sizeX * sizeY;

            // Count initial walls.
            int wallCount = 0;
            for (int x = 0; x < sizeX; x++)
                for (int y = 0; y < sizeY; y++)
                    if (IsTileWall(new Vector2Int(x, y))) wallCount++;

            // Always clear the center tile so BFS has a valid seed.
            Vector2Int center = new Vector2Int(sizeX / 2, sizeY / 2);
            if (IsTileWall(center))
            {
                ClearWallAt(center);
                wallCount--;
            }

            // BFS structures.
            HashSet<Vector2Int> floodFill = new HashSet<Vector2Int>();
            List<(Vector2Int pos, float noise)> frontier = new List<(Vector2Int pos, float noise)>();
            HashSet<Vector2Int> frontierSet = new HashSet<Vector2Int>();

            ExpandBFS(center, floodFill, frontier, frontierSet);

            // Connectivity loop.
            while (floodFill.Count + wallCount < totalTiles)
            {
                // ── Early-stop ──────────────────────────────────────────────
                float reachableRatio = (float)(floodFill.Count + wallCount) / totalTiles;
                float wallRatio = (float)wallCount / totalTiles;

                if (reachableRatio >= config.minConnectedCoveragePercent
                    && wallRatio <= config.maxWallPercentEarlyStop)
                    break;

                if (frontier.Count == 0) break; // Open space is already fully connected.

                // ── Remove the lowest-noise frontier wall ───────────────────
                int minIdx = 0;
                for (int i = 1; i < frontier.Count; i++)
                    if (frontier[i].noise < frontier[minIdx].noise) minIdx = i;

                Vector2Int minPos = frontier[minIdx].pos;
                frontier.RemoveAt(minIdx);
                frontierSet.Remove(minPos);

                ClearWallAt(minPos);
                wallCount--;

                ExpandBFS(minPos, floodFill, frontier, frontierSet);
            }

            // ── Hard cap: warn + despawn ────────────────────────────────────
            float finalWallRatio = (float)wallCount / totalTiles;
            if (finalWallRatio > config.maxWallCapPercent)
            {
                int excessCount = wallCount - Mathf.FloorToInt(totalTiles * config.maxWallCapPercent);
                Debug.LogWarning(
                    $"[WallPlacer] Wall coverage {finalWallRatio * 100f:F1}% still exceeds " +
                    $"{config.maxWallCapPercent * 100f:F0}% cap after the full connectivity pass. " +
                    $"Force-despawning {excessCount} wall(s) from lowest-noise positions.");

                // Collect and sort all remaining walls by noise value (ascending).
                List<(Vector2Int pos, float noise)> remaining = new List<(Vector2Int pos, float noise)>();
                for (int x = 0; x < sizeX; x++)
                    for (int y = 0; y < sizeY; y++)
                    {
                        Vector2Int p = new Vector2Int(x, y);
                        if (IsTileWall(p))
                            remaining.Add((p, noiseMap[x, y]));
                    }

                remaining.Sort((a, b) => a.noise.CompareTo(b.noise));

                for (int i = 0; i < excessCount && i < remaining.Count; i++)
                    ClearWallAt(remaining[i].pos);
            }
        }

        // ── Incremental BFS expansion ────────────────────────────────────────
        // Starts from 'start' (already removed from wall set / guaranteed open),
        // floods through all connected open tiles, and registers new wall neighbours
        // in the frontier list.
        private void ExpandBFS(
            Vector2Int start,
            HashSet<Vector2Int> floodFill,
            List<(Vector2Int pos, float noise)> frontier,
            HashSet<Vector2Int> frontierSet)
        {
            if (floodFill.Contains(start)) return;

            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            floodFill.Add(start);
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                foreach (Vector2Int dir in Directions)
                {
                    Vector2Int nb = current + dir;
                    if (nb.x < 0 || nb.x >= grid.Size.x || nb.y < 0 || nb.y >= grid.Size.y) continue;
                    if (floodFill.Contains(nb) || frontierSet.Contains(nb)) continue;

                    if (IsTileWall(nb))
                    {
                        frontierSet.Add(nb);
                        frontier.Add((nb, noiseMap[nb.x, nb.y]));
                    }
                    else
                    {
                        floodFill.Add(nb);
                        queue.Enqueue(nb);
                    }
                }
            }
        }

        // ── Tile helpers ─────────────────────────────────────────────────────
        private bool IsTileWall(Vector2Int pos)
        {
            List<GridPlaceable> tile = grid.GetTile(pos);
            if (tile == null) return false;
            foreach (GridPlaceable gp in tile)
                if (gp.Type == GridPlaceable.PlaceableType.Wall) return true;
            return false;
        }

        private void ClearWallAt(Vector2Int pos)
        {
            List<GridPlaceable> tile = grid.GetTile(pos);
            if (tile == null) return;
            // Snapshot to avoid modifying the list while iterating (Despawn calls RemoveFromGrid).
            GridPlaceable[] snapshot = tile.ToArray();
            foreach (GridPlaceable gp in snapshot)
                if (gp.Type == GridPlaceable.PlaceableType.Wall)
                    PoolingEntity.Despawn(gp.gameObject);
        }
    }
}
