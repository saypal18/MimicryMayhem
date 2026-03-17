using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class AbilityController
{
    [SerializeField] private float cooldown;
    private float lastActionTime = -Mathf.Infinity;
    [SerializeField] private float controlTime;
    private float lastControlTime = -Mathf.Infinity;
    private int controlTurns = 0;

    public Image cooldownImage;
    //public AbilityController(float cooldown)
    //{
    //    this.cooldown = cooldown;
    //    this.lastActionTime = -cooldown; // Allow immediate first move
    //}

    public bool CanAct()
    {
        return Time.time - lastActionTime >= cooldown && Time.time - lastControlTime >= controlTime && controlTurns <= 0;
    }

    public bool IsControlled() => controlTurns > 0;

    public void ConsumeControlTurn()
    {
        if (controlTurns > 0) controlTurns--;
    }

    public void Control(int turns = 1)
    {
        lastControlTime = Time.time;
        controlTurns = turns;
    }
    public bool Act(IAbility actionable)
    {
        if (!CanAct()) return false;
        if (!actionable.Perform()) return false;
        lastActionTime = Time.time;
        return true;
    }
    public void Initialize()
    {
        cooldownImage = null;
    }
    public void Update()
    {
        if (cooldownImage == null) return;
        cooldownImage.fillAmount = Mathf.Clamp01((Time.time - lastActionTime) / cooldown);
    }
}