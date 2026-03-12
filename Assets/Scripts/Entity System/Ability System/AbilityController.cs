using UnityEngine;
[System.Serializable]
public class AbilityController
{
    [SerializeField] private float cooldown;
    private float lastActionTime = -Mathf.Infinity;

    //public AbilityController(float cooldown)
    //{
    //    this.cooldown = cooldown;
    //    this.lastActionTime = -cooldown; // Allow immediate first move
    //}

    public bool CanAct()
    {
        return Time.time - lastActionTime >= cooldown;
    }

    public bool Act(IAbility actionable)
    {
        if (!CanAct()) return false;
        if (!actionable.Perform()) return false;
        lastActionTime = Time.time;
        return true;
    }
}