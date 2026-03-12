using System;

public interface ITick
{
    public Action OnTick { get; set; }
}