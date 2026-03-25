using UnityEngine;
using System.Collections.Generic;
using Unity.MLAgents.Policies;

public class AttackTileHighlighter : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject highlightPrefab;
    [SerializeField] private Color highlightColor = new Color(1, 0, 0, 0.4f);

    private Entity owner;
    private Grid grid;
    private EquippedItem equippedItem;
    private Vector2 mousePosition;
    private bool isMyTurn = false;

    private readonly List<GameObject> activeHighlights = new List<GameObject>();
    
    // Optimization tracking
    private Vector2Int lastDirection;
    private InventoryItem lastItem;
    private Vector2Int lastAgentPos;

    public void Initialize(Entity owner, Grid grid, EquippedItem equippedItem, ITick tick)
    {
        this.owner = owner;
        this.grid = grid;
        this.equippedItem = equippedItem;

        // Hook into the tick system to track when it's our turn
        tick.OnTick += () => isMyTurn = true;
        tick.OnPlayed += () =>
        {
            isMyTurn = false;
            ClearHighlights();
            ResetLastState();
        };
    }

    public void OnMouseMove(Vector2 mousePosition)
    {
        this.mousePosition = mousePosition;
    }

    private void Update()
    {
        if (owner == null || equippedItem == null || grid == null || highlightPrefab == null) return;

        // Requirement: Only during agent's turn
        if (!isMyTurn)
        {
            if (activeHighlights.Count > 0)
            {
                ClearHighlights();
                ResetLastState();
            }
            return;
        }

        // Rely on EquippedItem early state instead of ActiveAbility
        InventoryItem currentItem = equippedItem.Get();

        if (currentItem == null)
        {
            ClearHighlights();
            lastItem = null;
            return;
        }

        Vector3 directionVector = CalculateDirectionVector();
        Vector2Int cardinalDirection = GetCardinalDirection(directionVector);
        
        GridPlaceable gp = owner.agent.GetComponent<GridPlaceable>();
        if (gp == null) return;
        Vector2Int agentPos = gp.Position;

        // Optimization: Only update if direction, item, or agent position changed
        if (cardinalDirection != lastDirection || currentItem != lastItem || agentPos != lastAgentPos)
        {
            int range = GetImpactRangeFromItem(currentItem);
            UpdateHighlights(cardinalDirection, range);
            
            lastDirection = cardinalDirection;
            lastItem = currentItem;
            lastAgentPos = agentPos;
        }
    }

    private int GetImpactRangeFromItem(InventoryItem item)
    {
        if (item == null) return 0;
        
        // Weapon-specific ranges based on default values requested by user
        switch (item.itemType)
        {
            case ItemType.Sword: return 1;
            case ItemType.Bow: return 3;
            case ItemType.Shield: return 3;
            default: return 0;
        }
    }

    private void ResetLastState()
    {
        lastDirection = Vector2Int.zero;
        lastItem = null;
        lastAgentPos = new Vector2Int(-1, -1);
    }

    private void OnDisable()
    {
        ClearHighlights();
        ResetLastState();
    }

    private Vector3 CalculateDirectionVector()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -Camera.main.transform.position.z));
        return mouseWorldPos - owner.transform.position;
    }

    private Vector2Int GetCardinalDirection(Vector3 directionVector)
    {
        if (Mathf.Abs(directionVector.x) > Mathf.Abs(directionVector.y))
        {
            return directionVector.x > 0 ? Vector2Int.right : Vector2Int.left;
        }
        else
        {
            return directionVector.y > 0 ? Vector2Int.up : Vector2Int.down;
        }
    }

    private void UpdateHighlights(Vector2Int direction, int count)
    {
        GridPlaceable gp = owner.agent.GetComponent<GridPlaceable>();
        if (gp == null) return;

        Vector2Int currentPos = gp.Position;

        // Despawn all current using the project's pooling system
        ClearHighlights();

        // Show new
        for (int i = 1; i <= count; i++)
        {
            Vector2Int targetPos = currentPos + direction * i;
            
            // Check bounds
            if (targetPos.x < 0 || targetPos.x >= grid.Size.x || targetPos.y < 0 || targetPos.y >= grid.Size.y)
                break;

            // Requirement: Cannot attack through walls. Stop if we encounter a non-movable tile (wall).
            if (!grid.IsMovable(targetPos))
                break;

            // Spawn without parent to maintain global scale properly
            Vector3 worldPos = grid.GetWorldPosition(targetPos);
            GameObject hl = PoolingEntity.Spawn(highlightPrefab, worldPos, Quaternion.identity);
            if (hl == null) continue;

            // Global scale will effectively be 1,1,1 as it has no parent
            hl.transform.localScale = Vector3.one;
            hl.SetActive(true);
            activeHighlights.Add(hl);

            SpriteRenderer sr = hl.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = highlightColor;
        }
    }

    private void ClearHighlights()
    {
        foreach (var hl in activeHighlights)
        {
            if (hl != null) PoolingEntity.Despawn(hl);
        }
        activeHighlights.Clear();
    }
}
