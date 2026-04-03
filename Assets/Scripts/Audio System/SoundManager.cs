using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class SoundManager : MonoBehaviour
{
    [Header("Debug")]
    [Tooltip("Shows a warning when an event reference is null, otherwise it'll silently fail.")]
    [SerializeField] private bool strictAudioMode = false;
    public static bool StrictAudioMode;

    [Header("Audio Events")]
    [SerializeField] private AudioEventRegistry audioEvents;
    public static AudioEventRegistry Events { get; private set; }

    [Header("Banks")]
    [BankRef]
    [SerializeField] private List<string> requiredBanks = new List<string> { "Gameplay" };

    [Header("WebGL")]
    [Tooltip("Suspend FMOD mixer when the browser tab loses focus (WebGL only).")]
    [SerializeField] private bool suspendOnFocusLoss = true;

    private EventInstance musicInstance;
    private EventInstance ambienceInstance;

    void Awake()
    {
        StrictAudioMode = strictAudioMode;
        Events = audioEvents;
    }

    /// <summary>
    /// Returns true if the event reference is null. Logs a warning in strict mode.
    /// CallerMemberName and CallerFilePath are auto-filled by the compiler.
    /// </summary>
    public static bool CheckEventNull(
        EventReference eventRef,
        Object unityObject = null,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string filePath = "")
    {
        if (!eventRef.IsNull) return false;
        if (StrictAudioMode)
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            Debug.LogWarning($"[Audio] EventReference not assigned — called from {fileName}.{caller}()", unityObject);
        }
        return true;
    }

    /// <summary>
    /// Fire-and-forget one-shot with optional position and labelled parameters.
    /// </summary>
    public static void PlayOneShot(
        EventReference eventRef,
        Vector3? position = null,
        params (string name, string label)[] parameters)
    {
        if (Trainer.IsTraining) return;
        if (CheckEventNull(eventRef)) return;

        EventInstance instance = RuntimeManager.CreateInstance(eventRef);
        foreach (var (name, label) in parameters)
            instance.setParameterByNameWithLabel(name, label);
        if (position.HasValue)
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(position.Value));
        instance.start();
        instance.release();
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

        if (Trainer.IsTraining)
        {
            if (RuntimeManager.StudioSystem.getBus("bus:/", out var masterBus) == FMOD.RESULT.OK)
            {
                masterBus.setMute(true);
            }
        }
    }

    public IEnumerator WaitForBanksAndStart()
    {
        if (Trainer.IsTraining) yield break;

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
        if (Trainer.IsTraining) return;

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
        if (CheckEventNull(audioEvents.music, this)) return;

        musicInstance = RuntimeManager.CreateInstance(audioEvents.music);
        musicInstance.start();
    }

    private void StartAmbience()
    {
        if (CheckEventNull(audioEvents.ambience, this)) return;

        ambienceInstance = RuntimeManager.CreateInstance(audioEvents.ambience);
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

}
