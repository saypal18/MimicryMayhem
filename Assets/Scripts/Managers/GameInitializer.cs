using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private EntitySpawner entitySpawner;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PickupPlacer pickupPlacer;
    private void Start()
    {
        grid.Initialize();
        entitySpawner.Initialize(grid, pickupPlacer, transform);
        Entity entity = entitySpawner.Spawn();
        if (entity.TryGetComponent(out IMoveInputHandler moveHandler))
        {
            inputManager.InitializeMove(moveHandler);
        }
        pickupPlacer.Initialize(grid, transform);
        pickupPlacer.RandomPlacement(5);
    }
}