using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class PlayerActionHighlighter : MonoBehaviour
{
    [Header("Settings")]
    public GameObject moveHighlightPrefab;
    public GameObject attackHighlightPrefab;
    public GameObject targetHighlightPrefab;

    [SerializeField] private Color moveColor   = new Color(0,    0.5f, 1f,  0.3f);
    [SerializeField] private Color attackColor = new Color(1f,   0f,   0f,  0.3f);
    [SerializeField] private Color targetColor = new Color(1f,   1f,   0f,  0.4f);

    [Header("Faint Animation (non-hover)")]
    [SerializeField] private float faintAlpha    = 0.15f;   // base alpha when faint
    [SerializeField] private float faintMinAlpha = 0.08f;   // yoyo lower bound
    [SerializeField] private float faintMaxAlpha = 0.28f;   // yoyo upper bound
    [SerializeField] private float faintYoyoDuration = 1.2f;

    [Header("Hover Animation")]
    [SerializeField] private float hoverAlpha    = 0.75f;   // fixed alpha while hovered
    [SerializeField] private float hoverMinScale = 0.88f;
    [SerializeField] private float hoverMaxScale = 1.08f;
    [SerializeField] private float hoverYoyoDuration = 0.5f;

    [HideInInspector] public Transform highlightParent;

    // ── internal state ────────────────────────────────────────────────────
    private Entity        owner;
    private Grid          grid;
    private EquippedItem  equippedItem;
    private ITick         tick;
    private Camera        cam;
    private bool          isMyTurn = false;
    public  bool          IsMyTurn => isMyTurn;

    private InputManager  inputManager;
    private Entity        currentlyHoveredEnemy = null;

    private Dictionary<Vector2Int, GameObject> validMoveTiles       = new();
    private Dictionary<Entity,     GameObject> entityTargetHighlights = new();
    private List<GameObject>                   attackPathHighlights  = new();

    // Tracks whether each highlight's HOVERED state we last animated for.
    // true = hover animation running, false = faint animation running.
    private Dictionary<GameObject, bool> highlightHoverState = new();

    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    // ── public API ────────────────────────────────────────────────────────

    public void Initialize(Entity owner, Grid grid, EquippedItem equippedItem, ITick tick)
    {
        this.owner        = owner;
        this.grid         = grid;
        this.equippedItem = equippedItem;
        this.tick         = tick;
        this.cam          = Camera.main;

        tick.OnTick   += HandleOnTick;
        tick.OnPlayed += HandleOnPlayed;

        equippedItem.OnScroll += (_) => { };

        owner.inventory.OnItemAdded.AddListener(   (item, amount, index) => { });
        owner.inventory.OnItemRemoved.AddListener( (item, amount, index) => { });

        GridPlaceable gp = owner.GetComponent<GridPlaceable>();
        if (gp != null) gp.OnPositionChanged += (_) => { };
    }

    public void SetInputManager(InputManager manager)   => inputManager = manager;

    public void UpdateEnvironment(Grid newGrid, ITick newTick)
    {
        this.grid = newGrid;

        if (this.tick != null)
        {
            this.tick.OnTick   -= HandleOnTick;
            this.tick.OnPlayed -= HandleOnPlayed;
        }

        this.tick = newTick;

        if (this.tick != null)
        {
            this.tick.OnTick   += HandleOnTick;
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
        List<Entity> enemiesInVision = new();
        Vector2Int   currentPos      = owner.Position;
        int          visionRadius    = 5;

        Vector2      center    = grid.GetWorldPosition(currentPos);
        Collider2D[] colliders = Physics2D.OverlapBoxAll(
            center,
            new Vector2(visionRadius * 2 + 1, visionRadius * 2 + 1),
            0f);

        foreach (var col in colliders)
        {
            if (col.TryGetComponent(out Entity e) &&
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

        Entity  eHover     = null;
        int     clickLayer = LayerMask.NameToLayer("ClickDetection");
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

        Entity      hoveredEnemy  = (eHover != null && eHover != owner && eHover.TeamId != owner.TeamId) ? eHover : null;
        Vector2Int  hoveredGridPos = grid.GetGridPosition(mouseWorldPos);
        bool        isHoveringMove = hoveredEnemy == null && IsAdjacent(hoveredGridPos) && adjacentMoveTiles.Contains(hoveredGridPos);

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
            // Hide everything when an enemy is hovered
            foreach (var hl in validMoveTiles.Values)
                SetHighlightVisible(hl, false);
            return;
        }

        // Update existing
        foreach (var kvp in validMoveTiles)
        {
            bool inList    = tiles.Contains(kvp.Key);
            bool isHovered = hoveredTile.HasValue && kvp.Key == hoveredTile.Value;
            bool visible   = inList && (!hoveredTile.HasValue || isHovered);

            SetHighlightVisible(kvp.Value, visible);
            if (visible)
                ApplyAnimation(kvp.Value, moveColor, isHovered);
        }

        // Spawn new
        foreach (var pos in tiles)
        {
            if (validMoveTiles.ContainsKey(pos)) continue;

            bool isHovered = hoveredTile.HasValue && pos == hoveredTile.Value;
            bool visible   = !hoveredTile.HasValue || isHovered;

            GameObject hl = SpawnHighlight(pos, moveColor, moveHighlightPrefab);
            if (hl == null) continue;

            validMoveTiles[pos] = hl;
            hl.SetActive(visible);
            if (visible)
            {
                // Force-start animation fresh on spawn
                highlightHoverState[hl] = !isHovered; // guarantee mismatch → animation starts
                ApplyAnimation(hl, moveColor, isHovered);
            }
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
            foreach (var hl in attackPathHighlights)
                SetHighlightVisible(hl, false);
            ClearAttackPath();
            return;
        }

        // Update existing
        foreach (var kvp in entityTargetHighlights)
        {
            Entity     enemy     = kvp.Key;
            GameObject hl        = kvp.Value;
            bool       inList    = enemies.Contains(enemy);
            bool       isHovered = enemy == hoveredEnemy;
            bool       visible   = inList && (hoveredEnemy == null || isHovered);

            SetHighlightVisible(hl, visible);
            if (visible)
                ApplyAnimation(hl, targetColor, isHovered);
        }

        // Spawn new
        foreach (var enemy in enemies)
        {
            if (entityTargetHighlights.ContainsKey(enemy)) continue;

            bool       isHovered = enemy == hoveredEnemy;
            bool       visible   = hoveredEnemy == null || isHovered;

            GameObject hl = SpawnHighlight(enemy.Position, targetColor, targetHighlightPrefab, enemy.transform);
            if (hl == null) continue;

            entityTargetHighlights[enemy] = hl;
            hl.SetActive(visible);
            if (visible)
            {
                highlightHoverState[hl] = !isHovered; // force animation start
                ApplyAnimation(hl, targetColor, isHovered);
            }
        }

        // Attack path
        if (hoveredEnemy != null) ShowAttackPath(mouseWorldPos);
        else                      ClearAttackPath();
    }

    // ─────────────────────────────────────────────────────────────────────
    // DOTween animation helpers
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Starts or leaves running the correct animation for a highlight.
    /// Only restarts the tween when the hover state actually changes.
    /// </summary>
    private void ApplyAnimation(GameObject hl, Color baseColor, bool isHovered)
    {
        // Check if state has changed
        if (highlightHoverState.TryGetValue(hl, out bool wasHovered) && wasHovered == isHovered)
            return;   // same state → don't restart

        highlightHoverState[hl] = isHovered;

        SpriteRenderer sr = hl.GetComponentInChildren<SpriteRenderer>();
        if (sr == null) return;

        // Kill any running tweens on this object's SR and transform
        sr.DOKill();
        hl.transform.DOKill();

        if (isHovered)
        {
            // ── Hover: fixed alpha + scale yoyo ──────────────────────
            sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, hoverAlpha);

            hl.transform.localScale = Vector3.one;
            hl.transform.DOScale(hoverMaxScale, hoverYoyoDuration * 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(hl);  // auto-kill when GO is destroyed/pooled
        }
        else
        {
            // ── Faint: scale = 1 + alpha yoyo ────────────────────────
            hl.transform.localScale = Vector3.one;

            sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, faintAlpha);

            DOTween.To(
                    () => sr.color.a,
                    a  => sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, a),
                    faintMaxAlpha,
                    faintYoyoDuration * 0.5f)
                .From(faintMinAlpha)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetTarget(sr)   // so sr.DOKill() can find and stop this tween
                .SetLink(hl);
        }
    }

    private void SetHighlightVisible(GameObject hl, bool visible)
    {
        if (hl == null) return;
        if (hl.activeSelf == visible) return;

        hl.SetActive(visible);

        if (!visible)
        {
            // Kill tweens and reset so they restart cleanly when re-shown
            SpriteRenderer sr = hl.GetComponentInChildren<SpriteRenderer>();
            sr?.DOKill();
            hl.transform.DOKill();
            hl.transform.localScale = Vector3.one;
            highlightHoverState.Remove(hl);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Attack path
    // ─────────────────────────────────────────────────────────────────────

    private void ShowAttackPath(Vector3 mouseWorldPos)
    {
        ClearAttackPath();

        Vector2Int currentPos     = owner.Position;
        Vector3    directionVector = mouseWorldPos - owner.transform.position;

        Vector2Int dir = Mathf.Abs(directionVector.x) > Mathf.Abs(directionVector.y)
            ? new Vector2Int(directionVector.x > 0 ? 1 : -1, 0)
            : new Vector2Int(0, directionVector.y > 0 ? 1 : -1);

        InventoryItem item  = equippedItem.Get();
        int           range = (item is WeaponItem weapon) ? weapon.range : 0;

        for (int i = 1; i <= range; i++)
        {
            Vector2Int pathPos = currentPos + dir * i;
            if (!IsValidPosition(pathPos) || !grid.IsMovable(pathPos)) break;

            GameObject hl = SpawnHighlight(pathPos, attackColor, attackHighlightPrefab);
            if (hl == null) continue;

            // Force animation start (always hovered/bright for attack path)
            highlightHoverState[hl] = false; // guarantee mismatch so ApplyAnimation runs
            ApplyAnimation(hl, attackColor, true);

            attackPathHighlights.Add(hl);
        }
    }

    private void ClearAttackPath()
    {
        foreach (var hl in attackPathHighlights)
        {
            KillTweens(hl);
            PoolingEntity.Despawn(hl);
        }
        attackPathHighlights.Clear();
    }

    // ─────────────────────────────────────────────────────────────────────
    // Pooling / spawn / clear helpers
    // ─────────────────────────────────────────────────────────────────────

    private GameObject SpawnHighlight(
        Vector2Int pos, Color color, GameObject prefab, Transform customParent = null)
    {
        if (prefab == null) return null;

        Vector3    worldPos = grid.GetWorldPosition(pos);
        GameObject hl       = PoolingEntity.Spawn(prefab, customParent ?? highlightParent);
        if (hl == null) return null;

        hl.transform.localPosition = customParent != null ? Vector3.zero : (Vector3)worldPos;
        if (customParent == null) hl.transform.position = worldPos;
        hl.transform.localScale = Vector3.one;
        hl.SetActive(true);

        SpriteRenderer sr = hl.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.color = color;

        return hl;
    }

    private void ClearHighlights()
    {
        foreach (var hl in validMoveTiles.Values)       { KillTweens(hl); PoolingEntity.Despawn(hl); }
        foreach (var hl in entityTargetHighlights.Values){ KillTweens(hl); PoolingEntity.Despawn(hl); }
        ClearAttackPath();

        validMoveTiles.Clear();
        entityTargetHighlights.Clear();
        highlightHoverState.Clear();
    }

    private static void KillTweens(GameObject hl)
    {
        if (hl == null) return;
        hl.transform.DOKill();
        SpriteRenderer sr = hl.GetComponentInChildren<SpriteRenderer>();
        sr?.DOKill();
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
