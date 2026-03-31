using UnityEngine;
using System.Collections.Generic;

public class PlayerActionHighlighter : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject moveHighlightPrefab;
    public GameObject attackHighlightPrefab;
    public GameObject targetHighlightPrefab;

    [HideInInspector] public Transform highlightParent;

    // ── internal state ────────────────────────────────────────────────────
    private Entity owner;
    private Grid grid;
    private EquippedItem equippedItem;
    private ITick tick;
    private Camera cam;
    private bool isMyTurn = false;
    public bool IsMyTurn => isMyTurn;

    private InputManager inputManager;
    private Entity currentlyHoveredEnemy = null;

    private Dictionary<Vector2Int, GameObject> validMoveTiles = new();
    private Dictionary<Entity, GameObject> entityTargetHighlights = new();
    private List<GameObject> attackPathHighlights = new();

    // Tracks the last isHovering bool set on each highlight's Animator.
    // Prevents redundant SetBool calls every frame.
    private Dictionary<GameObject, bool> highlightHoverState = new();

    // Attack path cache — only rebuild when cardinal direction or target changes.
    private Vector2Int _lastAttackDir = new Vector2Int(int.MinValue, int.MinValue);
    private Entity _lastAttackedTarget = null;

    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    private static readonly int IsHoveringHash = Animator.StringToHash("isHovering");

    // ── public API ────────────────────────────────────────────────────────

    public void Initialize(Entity owner, Grid grid, EquippedItem equippedItem, ITick tick)
    {
        this.owner = owner;
        this.grid = grid;
        this.equippedItem = equippedItem;
        this.tick = tick;
        this.cam = Camera.main;

        tick.OnTick += HandleOnTick;
        tick.OnPlayed += HandleOnPlayed;

        equippedItem.OnScroll += (_) => { };
        owner.inventory.OnItemAdded.AddListener((item, amount, index) => { });
        owner.inventory.OnItemRemoved.AddListener((item, amount, index) => { });

        GridPlaceable gp = owner.GetComponent<GridPlaceable>();
        if (gp != null) gp.OnPositionChanged += (_) => { };
    }

    public void SetInputManager(InputManager manager) => inputManager = manager;

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

    // ── tick callbacks ────────────────────────────────────────────────────

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

    // ── Unity loop ────────────────────────────────────────────────────────

    private void Update()
    {
        if (!isMyTurn || grid == null || owner == null || cam == null || inputManager == null)
        {
            if (validMoveTiles.Count > 0 || entityTargetHighlights.Count > 0) ClearHighlights();
            return;
        }

        RefreshHighlights();
    }

    // ── highlight logic ───────────────────────────────────────────────────

    private void RefreshHighlights()
    {
        // --- Enemies in vision ---
        bool isHoldingWeapon = equippedItem.Get() is WeaponItem;
        List<Entity> enemiesInVision = new();
        Vector2Int currentPos = owner.Position;
        int visionRadius = 5;

        Vector2 center = grid.GetWorldPosition(currentPos);
        Collider2D[] colliders = Physics2D.OverlapBoxAll(
            center,
            new Vector2(visionRadius * 2 + 1, visionRadius * 2 + 1),
            0f);

        foreach (var col in colliders)
        {
            if (isHoldingWeapon && col.TryGetComponent(out Entity e) &&
                e != owner && e.TeamId != owner.TeamId && e.IsActiveForTurns)
                enemiesInVision.Add(e);
        }

        // --- Adjacent move tiles ---
        List<Vector2Int> adjacentMoveTiles = new();
        foreach (Vector2Int dir in Directions)
        {
            Vector2Int neighbor = currentPos + dir;
            if (!IsValidPosition(neighbor) || !grid.IsMovable(neighbor)) continue;

            bool occupied = false;
            foreach (var p in grid.GetTile(neighbor))
            {
                if (p.Type == GridPlaceable.PlaceableType.Entity && p.GetComponent<Entity>() != owner)
                { occupied = true; break; }
            }
            if (!occupied) adjacentMoveTiles.Add(neighbor);
        }

        // --- Mouse hover ---
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(
            new Vector3(inputManager.mousePosition.x, inputManager.mousePosition.y, -cam.transform.position.z));

        Entity eHover = null;
        int clickLayer = LayerMask.NameToLayer("ClickDetection");
        if (clickLayer != -1)
        {
            Collider2D c = Physics2D.OverlapPoint(mouseWorldPos, 1 << clickLayer);
            if (c != null && c.TryGetComponent(out Root root) && root.GO != null)
                eHover = root.GO.GetComponent<Entity>();
        }
        if (eHover == null)
        {
            Collider2D c = Physics2D.OverlapPoint(mouseWorldPos);
            c?.TryGetComponent(out eHover);
        }
        if (eHover != null && !eHover.IsActiveForTurns) eHover = null;

        Entity hoveredEnemy = (isHoldingWeapon && eHover != null && eHover != owner && eHover.TeamId != owner.TeamId) ? eHover : null;
        Vector2Int hoveredGridPos = grid.GetGridPosition(mouseWorldPos);
        bool isHoveringMove = hoveredEnemy == null && IsAdjacent(hoveredGridPos) && adjacentMoveTiles.Contains(hoveredGridPos);

        UpdateMoveHighlights(adjacentMoveTiles, isHoveringMove ? (Vector2Int?)hoveredGridPos : null);
        UpdateTargetHighlights(enemiesInVision, hoveredEnemy, mouseWorldPos, isHoveringMove);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Move highlights
    // ─────────────────────────────────────────────────────────────────────

    private void UpdateMoveHighlights(List<Vector2Int> tiles, Vector2Int? hoveredTile)
    {
        bool enemyIsHovered = currentlyHoveredEnemy != null;

        if (enemyIsHovered && hoveredTile == null)
        {
            foreach (var hl in validMoveTiles.Values)
                SetHighlightVisible(hl, false);
            return;
        }

        // Update existing
        foreach (var kvp in validMoveTiles)
        {
            bool isHovered = hoveredTile.HasValue && kvp.Key == hoveredTile.Value;
            bool visible = tiles.Contains(kvp.Key) && (!hoveredTile.HasValue || isHovered);

            SetHighlightVisible(kvp.Value, visible);
            if (visible) SetHovering(kvp.Value, isHovered);
        }

        // Spawn new
        foreach (var pos in tiles)
        {
            if (validMoveTiles.ContainsKey(pos)) continue;

            bool isHovered = hoveredTile.HasValue && pos == hoveredTile.Value;
            bool visible = !hoveredTile.HasValue || isHovered;

            GameObject hl = SpawnHighlight(pos, moveHighlightPrefab);
            if (hl == null) continue;

            validMoveTiles[pos] = hl;
            hl.SetActive(visible);
            if (visible) SetHovering(hl, isHovered);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Target highlights
    // ─────────────────────────────────────────────────────────────────────

    private void UpdateTargetHighlights(
        List<Entity> enemies, Entity hoveredEnemy, Vector3 mouseWorldPos, bool isHoveringMove)
    {
        currentlyHoveredEnemy = hoveredEnemy;

        if (isHoveringMove && hoveredEnemy == null)
        {
            foreach (var hl in entityTargetHighlights.Values)
                SetHighlightVisible(hl, false);
            if (attackPathHighlights.Count > 0) ResetAttackPath();
            return;
        }

        // Update existing
        foreach (var kvp in entityTargetHighlights)
        {
            Entity enemy = kvp.Key;
            GameObject hl = kvp.Value;
            bool isHovered = enemy == hoveredEnemy;
            bool visible = enemies.Contains(enemy) && (hoveredEnemy == null || isHovered);

            SetHighlightVisible(hl, visible);
            if (visible) SetHovering(hl, isHovered);
        }

        // Spawn new
        foreach (var enemy in enemies)
        {
            if (entityTargetHighlights.ContainsKey(enemy)) continue;

            bool isHovered = enemy == hoveredEnemy;
            bool visible = hoveredEnemy == null || isHovered;

            GameObject hl = SpawnHighlight(enemy.Position, targetHighlightPrefab, enemy.transform);
            if (hl == null) continue;

            entityTargetHighlights[enemy] = hl;
            hl.SetActive(visible);
            if (visible) SetHovering(hl, isHovered);
        }

        // Attack path
        if (hoveredEnemy != null)
            ShowAttackPath(mouseWorldPos);
        else if (attackPathHighlights.Count > 0)
            ResetAttackPath();
    }

    // ─────────────────────────────────────────────────────────────────────
    // Attack path  (no animation — static visuals only)
    // ─────────────────────────────────────────────────────────────────────

    private void ShowAttackPath(Vector3 mouseWorldPos)
    {
        Vector3 directionVector = mouseWorldPos - owner.transform.position;
        Vector2Int dir = Mathf.Abs(directionVector.x) > Mathf.Abs(directionVector.y)
            ? new Vector2Int(directionVector.x > 0 ? 1 : -1, 0)
            : new Vector2Int(0, directionVector.y > 0 ? 1 : -1);

        // Skip rebuild if nothing changed
        if (dir == _lastAttackDir && currentlyHoveredEnemy == _lastAttackedTarget) return;

        _lastAttackDir = dir;
        _lastAttackedTarget = currentlyHoveredEnemy;

        // Despawn old tiles WITHOUT touching the cache (already updated above)
        DespawnAttackHighlights();

        // Spawn new tiles
        Vector2Int currentPos = owner.Position;
        InventoryItem item = equippedItem.Get();
        int range = (item is WeaponItem weapon) ? weapon.range : 0;

        for (int i = 1; i <= range; i++)
        {
            Vector2Int pathPos = currentPos + dir * i;
            if (!IsValidPosition(pathPos) || !grid.IsMovable(pathPos)) break;

            GameObject hl = SpawnHighlight(pathPos, attackHighlightPrefab);
            if (hl != null) attackPathHighlights.Add(hl);
        }
    }

    /// <summary>Despawns attack highlights and resets the cache (call when attack path should no longer be shown).</summary>
    private void ResetAttackPath()
    {
        DespawnAttackHighlights();
        _lastAttackDir = new Vector2Int(int.MinValue, int.MinValue);
        _lastAttackedTarget = null;
    }

    /// <summary>Despawns attack highlights only — does NOT touch the direction/target cache.</summary>
    private void DespawnAttackHighlights()
    {
        foreach (var hl in attackPathHighlights) PoolingEntity.Despawn(hl);
        attackPathHighlights.Clear();
    }

    // ─────────────────────────────────────────────────────────────────────
    // Animator helper
    // ─────────────────────────────────────────────────────────────────────

    private void SetHovering(GameObject hl, bool isHovered)
    {
        if (highlightHoverState.TryGetValue(hl, out bool current) && current == isHovered) return;

        highlightHoverState[hl] = isHovered;

        Animator anim = hl.GetComponentInChildren<Animator>();
        if (anim != null) anim.SetBool(IsHoveringHash, isHovered);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Visibility helper
    // ─────────────────────────────────────────────────────────────────────

    private void SetHighlightVisible(GameObject hl, bool visible)
    {
        if (hl == null || hl.activeSelf == visible) return;

        hl.SetActive(visible);

        // Clear tracked state so SetHovering re-applies correctly when the object reappears
        if (!visible) highlightHoverState.Remove(hl);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Spawn / clear
    // ─────────────────────────────────────────────────────────────────────

    private GameObject SpawnHighlight(Vector2Int pos, GameObject prefab, Transform customParent = null)
    {
        if (prefab == null) return null;

        GameObject hl = PoolingEntity.Spawn(prefab, customParent ?? highlightParent);
        if (hl == null) return null;

        if (customParent != null)
            hl.transform.localPosition = Vector3.zero;
        else
            hl.transform.position = grid.GetWorldPosition(pos);

        hl.transform.localScale = Vector3.one;
        hl.SetActive(true);
        return hl;
    }

    private void ClearHighlights()
    {
        foreach (var hl in validMoveTiles.Values) PoolingEntity.Despawn(hl);
        foreach (var hl in entityTargetHighlights.Values) PoolingEntity.Despawn(hl);
        ResetAttackPath();

        validMoveTiles.Clear();
        entityTargetHighlights.Clear();
        highlightHoverState.Clear();
    }

    // ─────────────────────────────────────────────────────────────────────
    // Utility
    // ─────────────────────────────────────────────────────────────────────

    private bool IsValidPosition(Vector2Int pos) =>
        pos.x >= 0 && pos.x < grid.Size.x && pos.y >= 0 && pos.y < grid.Size.y;

    public bool IsAdjacent(Vector2Int pos)
    {
        Vector2Int current = owner.Position;
        foreach (Vector2Int dir in Directions)
            if (current + dir == pos) return true;
        return false;
    }

    public bool IsValidMoveTile(Vector2Int pos) => validMoveTiles.ContainsKey(pos);

    private void OnDisable() => ClearHighlights();
}
