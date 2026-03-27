using UnityEngine;
using DG.Tweening;
public class SmoothEntityMovement : IEntityMovement
{
    private float moveDuration = 0.2f;
    private int blocksToMove = 1;
    private GridPlaceable gridPlaceable;
    Transform transform;
    private MoveInfo moveInfo;
    public void Initialize(float duration, int blocks, GridPlaceable gridplaceable, MoveInfo moveInfo)
    {
        moveDuration = duration;
        blocksToMove = blocks;
        this.gridPlaceable = gridplaceable;
        transform = gridplaceable.transform;
        this.moveInfo = moveInfo;
    }
    // find the final v3 position
    // start tween movement towards it after cancelling any other transform tween
    // set gridplaceable move
    public bool Move(Vector2Int direction)
    {
        Vector2Int currentGridPosition = gridPlaceable.CurrentGrid.GetGridPosition(transform.position);
        gridPlaceable.SyncPosition(currentGridPosition);

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
        transform.DOKill();

        if (furthestReachable > 0)
        {
            float adjustedDuration = (float)furthestReachable * moveDuration / blocksToMove;
            Vector2Int newPosition = gridPlaceable.Position + direction * furthestReachable;
            if (gridPlaceable.MoveTo(newPosition))
            {
                moveInfo.CurrentDirection = direction;
                moveInfo.IsMoving = true;
                Vector3 targetPosition = gridPlaceable.CurrentGrid.GetWorldPosition(newPosition);
                transform.DOMove(targetPosition, adjustedDuration)
                    .SetEase(Ease.Linear)
                    .OnKill(() =>
                    {
                        if (moveInfo != null)
                        {
                            moveInfo.IsMoving = false;
                            DOTween.Kill(moveInfo);
                            DOVirtual.DelayedCall(0.05f, () =>
                            {
                                if (moveInfo != null) moveInfo.IsDashing = false;
                            }).SetId(moveInfo);
                        }
                    });
                return true;
            }
        }
        Vector3 originalPosition = transform.position;
        Vector3 shakeDirection = gridPlaceable.CurrentGrid.GetWorldPosition(gridPlaceable.Position + direction) - originalPosition;
        transform.DOShakePosition(0.2f, shakeDirection.normalized * 0.4f, 10, 0f).OnComplete(() => transform.position = originalPosition);
        return false;
    }

    //private void PlayMoveAnimation(Vector3 targetPosition, float moveDuration, Ease ease)
    //{
    //    transform.DOKill();
    //    transform.DOMove(targetPosition, moveDuration).SetEase(ease);
    //}
    //private void PlayCantMoveAnimation()
    //{
    //    Vector3 originalPosition = transform.position;
    //    transform.DOKill();
    //    transform.DOShakePosition(0.2f, 0.1f).OnComplete(() => transform.position = originalPosition);
    //}



    public void UpdateRange(int blocks)
    {
        this.blocksToMove = blocks;
    }
}