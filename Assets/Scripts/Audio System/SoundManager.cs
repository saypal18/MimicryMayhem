using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class SoundManager : MonoBehaviour
{
    [Header("Music & Ambience")]
    [SerializeField] private EventReference musicSoundEvent;
    [SerializeField] private EventReference ambienceSoundEvent;

    [Header("Banks")]
    [BankRef]
    [SerializeField] private List<string> requiredBanks = new List<string> { "Gameplay" };

    [Header("SFX")]
    [SerializeField] private EventReference levelStartSoundEvent;

    [Header("WebGL")]
    [Tooltip("Suspend FMOD mixer when the browser tab loses focus (WebGL only).")]
    [SerializeField] private bool suspendOnFocusLoss = true;

    private EventInstance musicInstance;
    private EventInstance ambienceInstance;

    public void LoadBanks()
    {
        foreach (var bank in requiredBanks)
        {
            try
            {
                if (!RuntimeManager.HasBankLoaded(bank))
                {
                    RuntimeManager.LoadBank(bank, true);
                }
            }
            catch (BankLoadException e)
            {
                Debug.LogError($"[Audio] Failed to load bank '{bank}': {e.Message}");
            }
        }
    }

    public IEnumerator WaitForBanksAndStart()
    {
        float timeout = 30f;
        float elapsed = 0f;
        while (!RuntimeManager.HaveAllBanksLoaded)
        {
            elapsed += Time.unscaledDeltaTime;
            if (elapsed >= timeout)
            {
                Debug.LogError("[Audio] Bank loading timed out after 30s");
                break;
            }
            yield return null;
        }

        StartBackgroundAudio();
    }

    public void StartBackgroundAudio()
    {
        StartMusic();
        StartAmbience();
    }

    public void StopBackgroundAudio()
    {
        StopMusic();
        StopAmbience();
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    private void OnApplicationFocus(bool focus)
    {
        if (!suspendOnFocusLoss) return;
        if (RuntimeManager.StudioSystem.isValid())
        {
            RuntimeManager.PauseAllEvents(!focus);

            if (!focus)
            {
                RuntimeManager.CoreSystem.mixerSuspend();
            }
            else
            {
                RuntimeManager.CoreSystem.mixerResume();
            }
        }
    }
#endif

    void OnDestroy()
    {
        StopMusic();
        StopAmbience();
    }

    private void StartMusic()
    {
        if (musicSoundEvent.IsNull) return;

        musicInstance = RuntimeManager.CreateInstance(musicSoundEvent);
        musicInstance.start();
    }

    private void StartAmbience()
    {
        if (ambienceSoundEvent.IsNull) return;

        ambienceInstance = RuntimeManager.CreateInstance(ambienceSoundEvent);
        ambienceInstance.start();
    }

    private void StopMusic()
    {
        if (!musicInstance.isValid()) return;

        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
    }

    private void StopAmbience()
    {
        if (!ambienceInstance.isValid()) return;

        ambienceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        ambienceInstance.release();
    }

    public void PlayLevelStart()
    {
        if (levelStartSoundEvent.IsNull) return;

        EventInstance instance = RuntimeManager.CreateInstance(levelStartSoundEvent);
        instance.start();
        instance.release();
    }
}
