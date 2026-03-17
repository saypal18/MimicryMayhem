using System;
using UnityEngine;

public class TimedTick : MonoBehaviour, ITick
{
    public Action OnTick { get; set; }
    public Action OnPlayed { get; set; }
    [SerializeField] private float tickInterval = 0.05f; // Time in seconds between ticks
    private float tickTimer;
    private void Update()
    {
        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            OnTick?.Invoke();
            tickTimer = 0f;
        }
    }
}