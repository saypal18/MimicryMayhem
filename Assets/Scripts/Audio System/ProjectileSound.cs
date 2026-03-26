using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class ProjectileSound : MonoBehaviour
{
    [SerializeField] private EventReference projectileSoundEvent;

    private EventInstance instance;

    void OnEnable()
    {
        if (projectileSoundEvent.IsNull) return;

        instance = RuntimeManager.CreateInstance(projectileSoundEvent);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
        instance.start();
    }

    void OnDisable()
    {
        if (instance.isValid())
        {
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
        }
    }

    void Update()
    {
        if (instance.isValid())
        {
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
        }
    }
}
