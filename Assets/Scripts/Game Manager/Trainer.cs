using UnityEngine;

public class Trainer : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private GameInitializer gameInitializerPrefab;
    [SerializeField] private int n = 32;
    [SerializeField] private Vector3 initialPosition;

    [SerializeField] private Vector2 gridSize;
    [SerializeField] private int fieldLength = 4;

    void Start()
    {
        // spawn n times and call reset env
        for (int i = 0; i < n; i++)
        {
            GameInitializer gameInitializer = Instantiate(gameInitializerPrefab);
            gameInitializer.transform.position = initialPosition + new Vector3((i / fieldLength) * gridSize.x, (i % fieldLength) * gridSize.y, 0);
            gameInitializer.inputManager = inputManager;
            gameInitializer.ResetEnvironment();
        }
    }
}