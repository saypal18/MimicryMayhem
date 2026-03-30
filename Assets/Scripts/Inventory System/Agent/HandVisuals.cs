using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attached to the weapon prefab to identify and color the hands.
/// </summary>
public class HandVisuals : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> hands = new List<SpriteRenderer>();

    public void SetColor(Color color)
    {
        foreach (var hand in hands)
        {
            if (hand != null)
            {
                hand.color = color;
            }
        }
    }
}
