using UnityEngine;
using System.Collections;

public class SimplePrefabAnimation : MonoBehaviour, IAnimation
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private float duration;

    public void Play()
    {
        Debug.Log("Playing animation");
        if (prefab != null)
        {
            StartCoroutine(PlayRoutine());
        }
    }
    public void Stop()
    {
    }

    private IEnumerator PlayRoutine()
    {
        prefab.SetActive(true);
        yield return new WaitForSeconds(duration);
        prefab.SetActive(false);
    }
}
