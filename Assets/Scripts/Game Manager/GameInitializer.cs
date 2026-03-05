using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private Grid grid;
    //[SerializeField] private EntitySpawner entitySpawner;

    private void Start()
    {
        grid.Initialize();
        //entitySpawner.Initialize(grid);
    }

}