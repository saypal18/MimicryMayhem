using UnityEngine;
using DG.Tweening;
public class SmoothEntityMovement : IEntityMovement
{
    private float moveDuration = 0.2f;
    private int blocksToMove = 1;
    private GridPlaceable gridPlaceable;
    Transform transform;
    public void Initialize(float duration, int blocks, GridPlaceable gridplaceable)
    {
        moveDuration = duration;
        blocksToMove = blocks;
        this.gridPlaceable = gridplaceable;
        transform = gridplaceable.transform;
    }
    // find the final v3 position
    // start tween movement towards it after cancelling any other transform tween
    // set gridplaceable move
    public void Move(Vector2Int direction)
    {
        Vector2Int newPosition = gridPlaceable.Position + direction * blocksToMove;
        Vector3 targetPosition = gridPlaceable.CurrentGrid.GetWorldPosition(newPosition);
        if (!gridPlaceable.MoveTo(newPosition)) return;
        transform.DOKill();
        transform.DOMove(targetPosition, moveDuration).SetEase(Ease.Linear);
    }

}   