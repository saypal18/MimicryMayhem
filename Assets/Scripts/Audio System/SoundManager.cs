using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Unity.MLAgents;

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
    public static bool CanPlayAudio { get; private set; } = true;

    void Awake()
    {
        // Detect training mode:
        // 1. Check if ML-Agents communicator is on (Python is training).
        // 2. Check if a Trainer component exists in the scene.
        bool isCommunicatorOn = Academy.IsInitialized && Academy.Instance.IsCommunicatorOn;
        bool hasTrainer = FindFirstObjectByType<Trainer>() != null;

        if (isCommunicatorOn || hasTrainer)
        {
            CanPlayAudio = false;
            Debug.Log("[Audio] Training mode detected. Disabling all audio events.");
        }
        else
        {
            CanPlayAudio = true;
        }
    }

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
        if (musicSoundEvent.IsNull || !CanPlayAudio) return;

        musicInstance = RuntimeManager.CreateInstance(musicSoundEvent);
        musicInstance.start();
    }

    private void StartAmbience()
    {
        if (ambienceSoundEvent.IsNull || !CanPlayAudio) return;

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
        if (levelStartSoundEvent.IsNull || !CanPlayAudio) return;

        EventInstance instance = RuntimeManager.CreateInstance(levelStartSoundEvent);
        instance.start();
        instance.release();
    }
}
