// using UnityEngine;
// using System.Collections;

// public class SnapColliderAnimation : MonoBehaviour, IDamageColliderAnimation
// {
//     private Grid grid;

//     public void Initialize(Grid grid)
//     {
//         this.grid = grid;
//     }

//     public void Play(float duration, Vector2Int position, Vector2Int direction, int damageBlocks, GameObject swordDamageCollider)
//     {
//         StartCoroutine(SnapRoutine(position, direction, damageBlocks, swordDamageCollider));
//     }

//     private IEnumerator SnapRoutine(Vector2Int position, Vector2Int direction, int damageBlocks, GameObject swordDamageCollider)
//     {
//         if (grid == null) 
//         {
//             Debug.LogWarning("SnapColliderAnimation not initialized with grid!");
//             yield break;
//         }

//         // Frame 1: start position
//         swordDamageCollider.transform.position = grid.GetWorldPosition(position + direction);

//         yield return null; // Wait for next frame (Frame 2)

//         // Frame 2: final position
//         swordDamageCollider.transform.position = grid.GetWorldPosition(position + direction * damageBlocks);
//     }
// }
