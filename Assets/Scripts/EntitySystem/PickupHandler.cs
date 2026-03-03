using System;
using UnityEngine;
public class PickupHandler
{
    private Transform transform;
    private Vector3 initialScale;
    public PickupHandler(Transform transform)
    {
        this.transform = transform;
        initialScale = transform.localScale;
    }
    public void Pickup(IPickable pickable)
    {
        transform.localScale += Vector3.one * 0.1f;
    }
    public void Reset()
    {
        transform.localScale = initialScale;
    }
}