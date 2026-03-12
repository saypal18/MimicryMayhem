using UnityEngine;
using System;
[Serializable]
public class ActiveAbility
{
    [SerializeField] private MeleeAttack meleeAttack;
    public IAbility ability => meleeAttack;
}