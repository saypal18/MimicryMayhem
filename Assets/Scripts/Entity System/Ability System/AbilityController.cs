using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class AbilityController
{
    [SerializeField] private float cooldown;
    private float lastActionTime = -Mathf.Infinity;
    [SerializeField] private float controlTime;
    private float lastControlTime = -Mathf.Infinity;

    public Image cooldownImage;
    //public AbilityController(float cooldown)
    //{
    //    this.cooldown = cooldown;
    //    this.lastActionTime = -cooldown; // Allow immediate first move
    //}

    public bool CanAct()
    {
        return Time.time - lastActionTime >= cooldown && Time.time - lastControlTime >= controlTime;
    }

    public void Control()
    {
        lastControlTime = Time.time;

    }
    public bool Act(IAbility actionable)
    {
        if (!CanAct()) return false;
        if (!actionable.Perform()) return false;
        lastActionTime = Time.time;
        return true;
    }

    public void Update()
    {
        if (cooldownImage == null) return;
        cooldownImage.fillAmount = Mathf.Clamp01((Time.time - lastActionTime) / cooldown);
    }
}