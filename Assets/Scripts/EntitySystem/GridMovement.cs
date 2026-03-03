using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
[System.Serializable]
public class GridMovement
{
    private Grid grid;
    private Transform entity;
    private Vector2Int position;
    [SerializeField] private float moveAnimationDuration = 0.1f;
    [SerializeField] private Ease ease = Ease.OutCubic;
    public bool IsMovable = true;
    public void Initialize(Grid grid, Transform entity)
    {
        this.grid = grid;
        this.entity = entity;
        this.position = grid.GetGridPosition(entity.position);
    }
    public void Move(Vector2Int direction)
    {
        if (DOTween.IsTweening(entity)) return;

        Vector2Int potentialPosition = position + direction;
        if (grid.IsMovable(potentialPosition))
        {
            position = potentialPosition;
            IsMovable = false;

            entity.DOMove(grid.GetWorldPosition(position), moveAnimationDuration).SetEase(ease).OnComplete(() => IsMovable = true);
        }
    }
    public void SetPosition(Vector2Int newPosition)
    {
        entity.DOKill();
        position = newPosition;
        entity.position = grid.GetWorldPosition(position);
        IsMovable = true;
    }
}