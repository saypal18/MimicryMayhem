using UnityEngine;

public class Entity : MonoBehaviour
{
    public GridMovement gridMovement;
    public PickupHandler pickupHandler;
    public void Initialize(Grid grid)
    {
        gridMovement.Initialize(grid, transform);
        pickupHandler = new PickupHandler(transform);
    }

}