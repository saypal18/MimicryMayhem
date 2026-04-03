using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class ProjectileSound : MonoBehaviour
{
    private EventInstance instance;

    void OnEnable()
    {
        if (Trainer.IsTraining || SoundManager.Events == null) return;
        if (SoundManager.CheckEventNull(SoundManager.Events.projectileFlight, this)) return;

        instance = RuntimeManager.CreateInstance(SoundManager.Events.projectileFlight);
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
