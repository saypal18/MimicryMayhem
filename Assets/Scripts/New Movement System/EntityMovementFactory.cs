using System;
using UnityEngine;

public class  EntityMovementFactory
{
    public IEntityMovement GetMovement(Type type)
    {
        if (type == typeof(MoveAbility))
        {
            return new SmoothEntityMovement();
        }
        else
        {
            throw new ArgumentException($"Unsupported movement type: {type}");
        }
    }
}