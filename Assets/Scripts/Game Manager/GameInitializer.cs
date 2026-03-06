using UnityEngine;
using WallSystem;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private EntitySpawner entitySpawner;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PickupPlacer pickupPlacer;
    [SerializeField] private WallPlacer wallPlacer;
    private void Start()
    {
        grid.Initialize();
        entitySpawner.Initialize(grid, inputManager);
        pickupPlacer.Initialize(grid);
        wallPlacer.Initialize(grid);
        entitySpawner.SpawnAtRandomPositions(2);
        pickupPlacer.SpawnAtRandomPositions(5);
        wallPlacer.SpawnAtRandomPositions(4);
    }

}