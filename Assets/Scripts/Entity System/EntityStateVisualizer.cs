using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Entity))]
public class EntityStateVisualizer : MonoBehaviour
{
    private Entity entity;

    [Header("State Icons (UI)")]
    [Tooltip("Image that changes color when the entity is moving.")]
    public Image isMovingImage;
    
    [Tooltip("Image that changes color when the entity is dashing.")]
    public Image isDashingImage;
    
    [Tooltip("Image that changes color when the entity is melee attacking.")]
    public Image isMeleeAttackingImage;
    
    [Tooltip("Image that changes color when the entity can move.")]
    public Image canMoveImage;

    [Header("Active State Colors")]
    public Color isMovingActiveColor = Color.blue;
    public Color isDashingActiveColor = Color.cyan;
    public Color isMeleeAttackingActiveColor = Color.red;
    public Color canMoveActiveColor = Color.green;

    [Header("Inactive State Color")]
    public Color inactiveColor = Color.white;

    private void Awake()
    {
        entity = GetComponent<Entity>();
    }

    private void LateUpdate()
    {
        if (entity == null) return;

        SetColor(isMovingImage, entity.moveInfo != null && entity.moveInfo.IsMoving, isMovingActiveColor);
        SetColor(isDashingImage, entity.moveInfo != null && entity.moveInfo.IsDashing, isDashingActiveColor);
        SetColor(isMeleeAttackingImage, entity.activeAbility != null && entity.activeAbility.IsMeleeAttacking, isMeleeAttackingActiveColor);
        SetColor(canMoveImage, entity.abilityController != null && entity.abilityController.CanAct(), canMoveActiveColor);
    }

    private void SetColor(Image image, bool isActive, Color activeColor)
    {
        if (image == null) return;
        image.color = isActive ? activeColor : inactiveColor;
    }
}
