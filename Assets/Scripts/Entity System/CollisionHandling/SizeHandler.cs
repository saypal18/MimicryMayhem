using UnityEngine;

[System.Serializable]
public class SizeHandler
{
    private Transform entity;
    [SerializeField]
    private Vector3 initialScale;
    public void Initialize(Transform entity, PickupHandler pickupHandler)
    {
        this.entity = entity;
        entity.localScale = initialScale;
        // initialScale = entity.localScale;
        pickupHandler.OnPickupCollected += IncreaseSize;
    }
    private void IncreaseSize(Pickup pickup)
    {
        // entity.localScale += Vector3.one * 0.001f;
    }

}