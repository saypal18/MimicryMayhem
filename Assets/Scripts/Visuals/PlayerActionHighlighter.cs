using UnityEngine;
using System.Collections.Generic;

public class PlayerActionHighlighter : MonoBehaviour
{
    [Header("Settings")]
    public GameObject moveHighlightPrefab;
    public GameObject attackHighlightPrefab;
    public GameObject targetHighlightPrefab;
    [SerializeField] private Color moveColor = new Color(0, 0.5f, 1f, 0.3f);
    [SerializeField] private Color attackColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private Color targetColor = new Color(1f, 1f, 0f, 0.4f);
    [SerializeField] private float hoverAlphaEnhancement = 0.5f;
    [SerializeField] private float faintAlphaMultiplier = 0.3f;
    [SerializeField] private Transform highlightParent;

    private Entity owner;
    private Grid grid;
    private EquippedItem equippedItem;
    private ITick tick;
    private Camera cam;
    private bool isMyTurn = false;
    public bool IsMyTurn => isMyTurn;

    private Dictionary<Vector2Int, GameObject> validMoveTiles = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, GameObject> validAttackTiles = new Dictionary<Vector2Int, GameObject>();
    private InputManager inputManager;
    
    // Directions
    private static readonly Vector2Int[] Directions = new[]
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    public void Initialize(Entity owner, Grid grid, EquippedItem equippedItem, ITick tick)
    {
        this.owner = owner;
        this.grid = grid;
        this.equippedItem = equippedItem;
        this.tick = tick;
        this.cam = Camera.main; // Fallback, but should ideally be injected if needed. 
        // Note: InputManager passes cam to moveInputHandler, but highlighter might need it too for hover check.

        tick.OnTick += HandleOnTick;
        tick.OnPlayed += HandleOnPlayed;
        
        equippedItem.OnScroll += (index) => {
            // Managed in Update()
        };

        owner.inventory.OnItemAdded.AddListener((item, amount, index) => {
            // Managed in Update()
        });
        owner.inventory.OnItemRemoved.AddListener((item, amount, index) => {
            // Managed in Update()
        });

        GridPlaceable gp = owner.GetComponent<GridPlaceable>();
        if (gp != null)
        {
            gp.OnPositionChanged += (pos) => {
                // Managed in Update()
            };
        }
        
        // Initial setup if we start on our turn (though tick will probably handle it)
    }

    private void HandleOnTick()
    {
        if (!enabled) return;
        isMyTurn = true;
    }

    private void HandleOnPlayed()
    {
        if (!enabled) return;
        isMyTurn = false;
        ClearHighlights();
    }

    public void SetInputManager(InputManager manager)
    {
        this.inputManager = manager;
    }

    public void UpdateEnvironment(Grid newGrid, ITick newTick)
    {
        this.grid = newGrid;
        
        if (this.tick != null)
        {
            this.tick.OnTick -= HandleOnTick;
            this.tick.OnPlayed -= HandleOnPlayed;
        }
        
        this.tick = newTick;
        
        if (this.tick != null)
        {
            this.tick.OnTick += HandleOnTick;
            this.tick.OnPlayed += HandleOnPlayed;
        }

        ClearHighlights();
    }

    private void Update()
    {
        if (!isMyTurn || grid == null || owner == null || cam == null || inputManager == null)
        {
            if (validMoveTiles.Count > 0 || validAttackTiles.Count > 0) ClearHighlights();
            return;
        }

        RefreshHighlights();
    }

    private void RefreshHighlights()
    {
        // 1. Determine potential targets (Enemy Entities in vision range 5)
        List<Entity> enemiesInVision = new List<Entity>();
        Vector2Int currentPos = owner.Position;
        int visionRadius = 5;

        // Note: This relies on Physics2D for finding enemies if we don't have an entity list.
        // For efficiency, we just overlap a box.
        float cellSize = 1f; // Assuming 1 unit per cell
        Vector2 center = grid.GetWorldPosition(currentPos);
        Collider2D[] colliders = Physics2D.OverlapBoxAll(center, new Vector2(visionRadius * 2 + 1, visionRadius * 2 + 1) * cellSize, 0f);
        
        foreach (var col in colliders)
        {
            if (col.TryGetComponent(out Entity e) && e != owner && e.TeamId != owner.TeamId)
            {
                enemiesInVision.Add(e);
            }
        }

        // 2. Identify Adjacent Move Tiles
        List<Vector2Int> adjacentMoveTiles = new List<Vector2Int>();
        foreach (Vector2Int dir in Directions)
        {
            Vector2Int neighbor = currentPos + dir;
            if (IsValidPosition(neighbor) && grid.IsMovable(neighbor))
            {
                bool occupiedByOther = false;
                foreach (var p in grid.GetTile(neighbor))
                {
                    if (p.Type == GridPlaceable.PlaceableType.Entity && p.GetComponent<Entity>() != owner)
                    {
                        occupiedByOther = true;
                        break;
                    }
                }
                if (!occupiedByOther) adjacentMoveTiles.Add(neighbor);
            }
        }

        // 3. Mouse Hover Check
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(new Vector3(inputManager.mousePosition.x, inputManager.mousePosition.y, -cam.transform.position.z));
        Collider2D hoveredCollider = Physics2D.OverlapPoint(mouseWorldPos);
        Entity hoveredEnemy = (hoveredCollider != null && hoveredCollider.TryGetComponent(out Entity eHover) && eHover != owner && eHover.TeamId != owner.TeamId) ? eHover : null;
        Vector2Int hoveredGridPos = grid.GetGridPosition(mouseWorldPos);
        bool isHoveringMove = hoveredEnemy == null && IsAdjacent(hoveredGridPos) && adjacentMoveTiles.Contains(hoveredGridPos);

        // 4. Update Visuals (Faint vs Prominent)
        // Clear old ones first to keep it simple, or reuse. For simplicity, we reuse.
        
        // Handle Move Highlights
        UpdateMoveHighlights(adjacentMoveTiles, isHoveringMove ? (Vector2Int?)hoveredGridPos : null);

        // Handle Target Highlights
        UpdateTargetHighlights(enemiesInVision, hoveredEnemy);
    }

    private void UpdateMoveHighlights(List<Vector2Int> tiles, Vector2Int? hoveredTile)
    {
        // Hide all if we are hovering an enemy
        if (hoveredTile == null && currentlyHoveredEnemy != null)
        {
            foreach (var hl in validMoveTiles.Values) hl.SetActive(false);
        }
        else
        {
            // If hovering a move tile, hide all other move highlights
            foreach (var kvp in validMoveTiles)
            {
                bool isTarget = hoveredTile.HasValue && kvp.Key == hoveredTile.Value;
                if (hoveredTile.HasValue && !isTarget)
                {
                    kvp.Value.SetActive(false);
                }
                else
                {
                    kvp.Value.SetActive(tiles.Contains(kvp.Key));
                    if (kvp.Value.activeSelf)
                    {
                        SetHighlightAlpha(kvp.Value, moveColor, hoveredTile.HasValue ? 1.0f : faintAlphaMultiplier);
                    }
                }
            }

            // Spawn new ones if missing
            foreach (var pos in tiles)
            {
                if (!validMoveTiles.ContainsKey(pos))
                {
                    GameObject hl = SpawnHighlight(pos, moveColor, moveHighlightPrefab);
                    if (hl != null) 
                    {
                        validMoveTiles[pos] = hl;
                        SetHighlightAlpha(hl, moveColor, hoveredTile == pos ? 1.0f : faintAlphaMultiplier);
                        hl.SetActive(hoveredTile == null || hoveredTile == pos);
                    }
                }
            }
        }
    }

    private Entity currentlyHoveredEnemy = null;

    private void UpdateTargetHighlights(List<Entity> enemies, Entity hoveredEnemy)
    {
        currentlyHoveredEnemy = hoveredEnemy;

        // If hovering a move tile, hide all target highlights
        bool isHoveringMove = IsAdjacent(grid.GetGridPosition(cam.ScreenToWorldPoint(new Vector3(inputManager.mousePosition.x, inputManager.mousePosition.y, -cam.transform.position.z))));
        
        if (isHoveringMove && hoveredEnemy == null)
        {
            foreach (var hl in validAttackTiles.Values) hl.SetActive(false);
            ClearAttackPath();
        }
        else
        {
            // If hovering an enemy, hide all other target highlights
            foreach (var kvp in entityTargetHighlights)
            {
                Entity enemy = kvp.Key;
                GameObject hl = kvp.Value;
                
                if (hoveredEnemy != null && enemy != hoveredEnemy)
                {
                    hl.SetActive(false);
                }
                else
                {
                    hl.SetActive(enemies.Contains(enemy));
                    if (hl.activeSelf)
                    {
                        SetHighlightAlpha(hl, targetColor, hoveredEnemy != null ? 1.0f : faintAlphaMultiplier);
                    }
                }
            }

            // Spawn new targets
            foreach (var enemy in enemies)
            {
                if (!entityTargetHighlights.ContainsKey(enemy))
                {
                    GameObject hl = SpawnHighlight(enemy.Position, targetColor, targetHighlightPrefab, enemy.transform);
                    if (hl != null)
                    {
                        entityTargetHighlights[enemy] = hl;
                        SetHighlightAlpha(hl, targetColor, enemy == hoveredEnemy ? 1.0f : faintAlphaMultiplier);
                        hl.SetActive(hoveredEnemy == null || hoveredEnemy == enemy);
                    }
                }
            }

            // Attack Path logic
            if (hoveredEnemy != null)
            {
                ShowAttackPath(hoveredEnemy.Position);
            }
            else
            {
                ClearAttackPath();
            }
        }
    }

    private Dictionary<Entity, GameObject> entityTargetHighlights = new Dictionary<Entity, GameObject>();
    private List<GameObject> attackPathHighlights = new List<GameObject>();

    private void ShowAttackPath(Vector2Int targetPos)
    {
        ClearAttackPath();
        Vector2Int currentPos = owner.Position;
        Vector2Int diff = targetPos - currentPos;
        
        // Closest cardinal direction
        Vector2Int dir = Vector2Int.zero;
        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y)) dir = new Vector2Int(diff.x > 0 ? 1 : -1, 0);
        else dir = new Vector2Int(0, diff.y > 0 ? 1 : -1);

        InventoryItem item = equippedItem.Get();
        int range = (item is WeaponItem weapon) ? weapon.range : 0;
        
        for (int i = 1; i <= range; i++)
        {
            Vector2Int pathPos = currentPos + dir * i;
            if (!IsValidPosition(pathPos) || !grid.IsMovable(pathPos)) break;
            
            GameObject hl = SpawnHighlight(pathPos, attackColor, attackHighlightPrefab);
            if (hl != null)
            {
                SetHighlightAlpha(hl, attackColor, 1.0f);
                attackPathHighlights.Add(hl);
            }
        }
    }

    private void ClearAttackPath()
    {
        foreach (var hl in attackPathHighlights) PoolingEntity.Despawn(hl);
        attackPathHighlights.Clear();
    }

    private void SetHighlightAlpha(GameObject hl, Color baseColor, float alphaFactor)
    {
        SpriteRenderer sr = hl.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * alphaFactor); 
    }

    private GameObject SpawnHighlight(Vector2Int pos, Color color, GameObject prefab, Transform customParent = null)
    {
        if (prefab == null) return null;
        Vector3 worldPos = grid.GetWorldPosition(pos);
        GameObject hl = PoolingEntity.Spawn(prefab, customParent != null ? customParent : highlightParent);
        if (hl != null)
        {
            if (customParent != null)
            {
                hl.transform.localPosition = Vector3.zero;
            }
            else
            {
                hl.transform.position = worldPos;
            }
            hl.transform.localScale = Vector3.one;
            hl.SetActive(true);
            SpriteRenderer sr = hl.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = color;
        }
        return hl;
    }

    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < grid.Size.x && pos.y >= 0 && pos.y < grid.Size.y;
    }

    private void ClearHighlights()
    {
        foreach (var hl in validMoveTiles.Values) PoolingEntity.Despawn(hl);
        foreach (var hl in entityTargetHighlights.Values) PoolingEntity.Despawn(hl);
        ClearAttackPath();
        validMoveTiles.Clear();
        entityTargetHighlights.Clear();
    }

    public bool IsAdjacent(Vector2Int pos)
    {
        Vector2Int currentPos = owner.Position;
        foreach (Vector2Int dir in Directions)
        {
            if (currentPos + dir == pos) return true;
        }
        return false;
    }

    private void OnDisable()
    {
        ClearHighlights();
    }

    public bool IsValidMoveTile(Vector2Int pos) => validMoveTiles.ContainsKey(pos);
}
