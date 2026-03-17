using System;

public class TurnedTick : ITick
{
    public Action OnTick { get; set; }
    public Action OnPlayed { get; set; }

    public void TriggerTick()
    {
        OnTick?.Invoke();
    }
}