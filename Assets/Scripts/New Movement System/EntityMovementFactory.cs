using System;
using UnityEngine;

public class  EntityMovementFactory
{
    public IEntityMovement GetMovement(Type type)
    {
        return new SmoothEntityMovement();

        //if (type == typeof(MoveAbility))
        //{
        //    return new SmoothEntityMovement();
        //}
        //else if (type == typeof(UnifiedDamageResolver))
        //{
        //    return new SmoothEntityMovement();
        //}
        //else if (type == typeof(DashAttack))
        //{
        //    return new SmoothEntityMovement();
        //}
        //else
        //{
        //    throw new ArgumentException($"Unsupported movement type: {type}");
        //}
    }
}