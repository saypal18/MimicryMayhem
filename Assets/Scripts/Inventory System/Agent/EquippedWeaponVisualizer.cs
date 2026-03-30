using UnityEngine;

/// <summary>
/// Manages instantiation and coloring of weapon prefabs based on the entity's current state.
/// This replaces the old EquippedItemVisuals script.
/// </summary>
public class EquippedWeaponVisualizer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Entity entity;
    [SerializeField] private EquippedItem equippedItem;
    [SerializeField] private Transform weaponParent;

    [Header("Colors (Entity Type)")]
    [SerializeField] private Color playerColor = Color.white;
    [SerializeField] private Color bossColor = Color.red;
    [SerializeField] private Color ruleBasedColor = Color.green;
    [SerializeField] private Color mlAgentColor = Color.yellow;

    private GameObject currentInstance;
    private HandVisuals currentHands;
    private InventoryItem currentItem;
    private Color lastColor;

    private void Awake()
    {
        if (entity == null) entity = GetComponentInParent<Entity>();
        if (equippedItem == null) equippedItem = GetComponentInParent<EquippedItem>();
        if (weaponParent == null) weaponParent = this.transform;
    }

    private void Update()
    {
        if (equippedItem == null || entity == null) return;

        InventoryItem newItem = equippedItem.Get();
        Color newColor = GetCurrentBeingColor();

        // If the item or the entity state (color) changed, update the visual
        if (newItem != currentItem || newColor != lastColor)
        {
            UpdateVisual(newItem, newColor);
            currentItem = newItem;
            lastColor = newColor;
        }
    }

    private void UpdateVisual(InventoryItem item, Color handColor)
    {
        // Cleanup old instance
        if (currentInstance != null)
        {
            PoolingEntity.Despawn(currentInstance);
            currentInstance = null;
            currentHands = null;
        }

        if (item is WeaponItem weaponItem && weaponItem.equipPrefab != null)
        {
            currentInstance = PoolingEntity.Spawn(weaponItem.equipPrefab, weaponParent);
            currentInstance.transform.localPosition = Vector3.zero;
            currentInstance.transform.localRotation = Quaternion.identity;

            currentHands = currentInstance.GetComponent<HandVisuals>();
            if (currentHands != null)
            {
                currentHands.SetColor(handColor);
            }
        }
    }

    private Color GetCurrentBeingColor()
    {
        if (entity.IsBoss) return bossColor;
        if (entity.IsPlayer) return playerColor;
        if (entity.agent != null && entity.agent.isRuleBased) return ruleBasedColor;
        return mlAgentColor;
    }
}
