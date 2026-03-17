using System;

public interface ITick
{
    public Action OnTick { get; set; }
    public Action OnPlayed { get; set; }
}