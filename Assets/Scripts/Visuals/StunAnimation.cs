using UnityEngine;

public class StunAnimation : MonoBehaviour, IAnimation
{
    [SerializeField] private GameObject prefab;

    public void Play()
    {
        if (prefab != null)
        {
            prefab.SetActive(true);
        }
    }

    public void Stop()
    {
        if (prefab != null)
        {
            prefab.SetActive(false);
        }
    }
}
