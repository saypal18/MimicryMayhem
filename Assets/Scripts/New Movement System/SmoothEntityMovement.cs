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
        int furthestReachable = 0;
        for (int i = 1; i <= blocksToMove; i++)
        {
            Vector2Int checkPosition = gridPlaceable.Position + direction * i;
            if (gridPlaceable.CurrentGrid.IsMovable(checkPosition))
            {
                furthestReachable = i;
            }
            else
            {
                break;
            }
        }

        if (furthestReachable > 0)
        {
            float adjustedDuration = (float)furthestReachable * moveDuration / blocksToMove;
            Vector2Int newPosition = gridPlaceable.Position + direction * furthestReachable;
            if (gridPlaceable.MoveTo(newPosition))
            {
                Vector3 targetPosition = gridPlaceable.CurrentGrid.GetWorldPosition(newPosition);
                transform.DOKill();
                transform.DOMove(targetPosition, adjustedDuration).SetEase(Ease.Linear);
            }
        }
    }

}   