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
    [SerializeField] private InterfaceReference<IAnimation> stunAnimation;
    private MoveInfo moveInfo;
    public Image cooldownImage;
    //public AbilityController(float cooldown)
    //{
    //    this.cooldown = cooldown;
    //    this.lastActionTime = -cooldown; // Allow immediate first move
    //}

    public bool CanAct()
    {
        return Time.time - lastActionTime >= cooldown && Time.time - lastControlTime >= controlTime && controlTurns <= 0 && !moveInfo.IsMoving;
    }

    public bool IsControlled() => controlTurns > 0;

    public void ConsumeControlTurn()
    {
        if (controlTurns > 0)
        {
            controlTurns--;
            if (controlTurns == 0)
            {
                stunAnimation.Value?.Stop();
            }
        }
    }

    public void Control(int turns = 1)
    {
        lastControlTime = Time.time;
        controlTurns = turns;

        if (controlTurns > 0)
        {
            stunAnimation.Value?.Play();
        }
    }
    public bool Act(IAbility actionable)
    {
        if (!CanAct()) return false;
        if (!actionable.Perform()) return false;
        lastActionTime = Time.time;
        return true;
    }
    public void Initialize(MoveInfo moveInfo)
    {
        cooldownImage = null;
        this.moveInfo = moveInfo;
    }
    public void Update()
    {
        if (cooldownImage == null) return;
        cooldownImage.fillAmount = Mathf.Clamp01((Time.time - lastActionTime) / cooldown);
    }
}