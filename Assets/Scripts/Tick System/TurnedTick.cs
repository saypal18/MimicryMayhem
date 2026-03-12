using UnityEngine;
using System;
public class TurnedTick : MonoBehaviour, ITick
{
    public Action OnTick { get; set; }

    public void TriggerTick()
    {
        OnTick?.Invoke();
    }
}