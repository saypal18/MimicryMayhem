using UnityEngine;
using FMODUnity;

[CreateAssetMenu(menuName = "Audio/Audio Event Registry", fileName = "AudioEventRegistry")]
public class AudioEventRegistry : ScriptableObject
{
    [Header("Combat")]
    public EventReference attack;
    public EventReference impact;
    public EventReference gripReduced;
    public EventReference knockback;
    public EventReference entityDeath;
    public EventReference projectileFlight;

    [Header("Items")]
    public EventReference pickup;
    public EventReference steal;
    public EventReference drop;
    public EventReference keyPickup;
    public EventReference keyDrop;

    [Header("Footstep")]
    public EventReference gridMovement;

    [Header("Enemy")]
    public EventReference activationBark;
    public EventReference bossPresence;

    [Header("UI")]
    public EventReference weaponSwitch;
    public EventReference areaTransition;

    [Header("Music & Ambience")]
    public EventReference music;
    public EventReference ambience;
    public EventReference levelCompleted;
}
