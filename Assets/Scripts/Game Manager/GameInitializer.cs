using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private EntitySpawner entitySpawner;
    [SerializeField] private InputManager inputManager;
    private void Start()
    {
        grid.Initialize();
        entitySpawner.Initialize(grid, inputManager);
        entitySpawner.SpawnAtRandomPosition(2);
    }

}