using System;
using UnityEngine;

public class CollisionResolver : MonoBehaviour
{
    public Action<GameObject> OnCollision;
    public void Initialize()
    {
        OnCollision = null;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Root root) && root != gameObject)
        {
            OnCollision?.Invoke(root.GO);
        }
    }

}