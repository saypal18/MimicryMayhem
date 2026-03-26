using UnityEngine;
using System.Collections.Generic;

public class PlayerActionHighlighter : MonoBehaviour
{
    [Header("Settings")]
    public GameObject moveHighlightPrefab;
    public GameObject attackHighlightPrefab;
    [SerializeField] private Color moveColor = new Color(0, 0.5f, 1f, 0.3f);
    [SerializeField] private Color attackColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private float hoverAlphaEnhancement = 0.5f;

    private Entity owner;
    private Grid grid;
    private EquippedItem equippedItem;
    private ITick tick;
    private bool isMyTurn = false;
    public bool IsMyTurn => isMyTurn;

    private Dictionary<Vector2Int, GameObject> validMoveTiles = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, GameObject> validAttackTiles = new Dictionary<Vector2Int, GameObject>();
    
    private Vector2Int? currentlyHoveredTile = null;
    
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

        tick.OnTick += () => {
            if (!enabled) return;
            isMyTurn = true;
            UpdateActionTiles();
        };
        tick.OnPlayed += () => {
            if (!enabled) return;
            isMyTurn = false;
            ClearHighlights();
        };
        
        equippedItem.OnScroll += (index) => {
            if (!enabled) return;
            if (isMyTurn)
            {
                UpdateActionTiles();
            }
        };

        owner.inventory.OnItemAdded.AddListener((item, amount, index) => {
            if (!enabled) return;
            if (isMyTurn) UpdateActionTiles();
        });
        owner.inventory.OnItemRemoved.AddListener((item, amount, index) => {
            if (!enabled) return;
            if (isMyTurn) UpdateActionTiles();
        });

        GridPlaceable gp = owner.GetComponent<GridPlaceable>();
        if (gp != null)
        {
            gp.OnPositionChanged += (pos) => {
                if (!enabled) return;
                if (isMyTurn) UpdateActionTiles();
            };
        }
        
        // Initial setup if we start on our turn (though tick will probably handle it)
    }

    public void OnMouseMove(Vector2 mousePosition)
    {
        if (!isMyTurn || grid == null || owner == null) return;
        
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z));
        Vector2Int gridPos = grid.GetGridPosition(mouseWorldPos);

        if (currentlyHoveredTile != gridPos)
        {
            // Reset old hover alpha
            if (currentlyHoveredTile.HasValue)
            {
                SetTileAlpha(currentlyHoveredTile.Value, false);
            }
            
            // Set new hover alpha
            currentlyHoveredTile = gridPos;
            SetTileAlpha(currentlyHoveredTile.Value, true);
        }
    }

    private void SetTileAlpha(Vector2Int pos, bool isHovered)
    {
        float addAlpha = isHovered ? hoverAlphaEnhancement : 0f;
        
        if (validAttackTiles.ContainsKey(pos))
        {
            GridPlaceable gp = owner.agent.GetComponent<GridPlaceable>();
            if (gp != null)
            {
                Vector2Int currentPos = gp.Position;
                Vector2Int hoverDir = pos - currentPos;
                Vector2Int dirUnit = new Vector2Int(
                    hoverDir.x == 0 ? 0 : (hoverDir.x > 0 ? 1 : -1),
                    hoverDir.y == 0 ? 0 : (hoverDir.y > 0 ? 1 : -1)
                );

                foreach (var kvp in validAttackTiles)
                {
                    Vector2Int attackPos = kvp.Key;
                    Vector2Int attackDir = attackPos - currentPos;
                    Vector2Int aUnit = new Vector2Int(
                        attackDir.x == 0 ? 0 : (attackDir.x > 0 ? 1 : -1),
                        attackDir.y == 0 ? 0 : (attackDir.y > 0 ? 1 : -1)
                    );

                    if (aUnit == dirUnit)
                    {
                        SpriteRenderer sr = kvp.Value.GetComponentInChildren<SpriteRenderer>();
                        if (sr != null) sr.color = new Color(attackColor.r, attackColor.g, attackColor.b, attackColor.a + addAlpha);
                    }
                }
            }
        }
        
        if (validMoveTiles.TryGetValue(pos, out GameObject movHl))
        {
            SpriteRenderer sr = movHl.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = new Color(moveColor.r, moveColor.g, moveColor.b, moveColor.a + addAlpha);
        }
    }

    public void UpdateActionTiles()
    {
        ClearHighlights();
        
        GridPlaceable gp = owner.agent.GetComponent<GridPlaceable>();
        if (gp == null) return;
        Vector2Int currentPos = gp.Position;

        // Calculate Move Tiles (1 step cardinal)
        foreach (Vector2Int dir in Directions)
        {
            Vector2Int targetPos = currentPos + dir;
            if (IsValidPosition(targetPos) && grid.IsMovable(targetPos))
            {
                // Check if moving into Entity is allowed? Currently we mask on Wall.
                bool hasEnemy = false;
                foreach (var p in grid.GetTile(targetPos))
                {
                    if (p.Type == GridPlaceable.PlaceableType.Entity && p != gp) hasEnemy = true;
                }
                
                // If there's an enemy, we usually can't "move" into it, but we can melee attack.
                if (!hasEnemy)
                {
                    GameObject hl = SpawnHighlight(targetPos, moveColor, moveHighlightPrefab);
                    if (hl != null) validMoveTiles[targetPos] = hl;
                }
            }
        }

        // Calculate Attack Tiles (up to weapon range cardinal)
        InventoryItem currentItem = equippedItem.Get();
        int range = GetImpactRangeFromItem(currentItem);
        if (range > 0)
        {
            foreach (Vector2Int dir in Directions)
            {
                for (int i = 1; i <= range; i++)
                {
                    Vector2Int targetPos = currentPos + dir * i;
                    if (!IsValidPosition(targetPos)) break;
                    if (!grid.IsMovable(targetPos)) break; // Line of sight blocked by wall
                    
                    GameObject hl = SpawnHighlight(targetPos, attackColor, attackHighlightPrefab);
                    if (hl != null) 
                    {
                        validAttackTiles[targetPos] = hl;
                    }
                }
            }
        }
    }

    private GameObject SpawnHighlight(Vector2Int pos, Color color, GameObject prefab)
    {
        if (prefab == null) return null;
        Vector3 worldPos = grid.GetWorldPosition(pos);
        GameObject hl = PoolingEntity.Spawn(prefab, worldPos, Quaternion.identity);
        if (hl != null)
        {
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
        foreach (var kvp in validMoveTiles) PoolingEntity.Despawn(kvp.Value);
        foreach (var kvp in validAttackTiles) PoolingEntity.Despawn(kvp.Value);
        validMoveTiles.Clear();
        validAttackTiles.Clear();
        currentlyHoveredTile = null;
    }

    private int GetImpactRangeFromItem(InventoryItem item)
    {
        if (item == null) return 0;
        switch (item.itemType)
        {
            case ItemType.Sword: return 1;
            case ItemType.Bow: return 3;
            case ItemType.Shield: return 3;
            default: return 0;
        }
    }

    private void OnDisable()
    {
        ClearHighlights();
    }

    public bool IsValidMoveTile(Vector2Int pos) => validMoveTiles.ContainsKey(pos);
    public bool IsValidAttackTile(Vector2Int pos) => validAttackTiles.ContainsKey(pos);
}
