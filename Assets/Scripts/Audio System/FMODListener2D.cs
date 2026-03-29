using UnityEngine;
using FMODUnity;

public class FMODListener2D : MonoBehaviour
{
    void Update()
    {
        Vector3 pos = transform.position;
        pos.z = 0f;
        RuntimeManager.StudioSystem.setListenerAttributes(
            0,
            RuntimeUtils.To3DAttributes(pos)
        );
    }
}
