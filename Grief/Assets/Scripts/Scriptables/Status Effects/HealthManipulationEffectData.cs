using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Status Effects/Health Manipulation Effect")]
public class HealthManipulationEffectData : StatusEffectData
{
    [Space(15)]
    [Header("Health Manipulation Effect Variables")]
    [SerializeField] private float healthEffectValue;
    [SerializeField] private bool isDamage = false;

    public override bool CanApplyStatusEffect(IStatusEffectTarget statusEffectTarget)
    {
        return statusEffectTarget.StatusEffectHolder.transform.TryGetComponent<IHealth>(out _);
    }

    public override void ApplyStatusEffect(IStatusEffectTarget statusEffectTarget)
    {
        if (isDamage)
        {
            statusEffectTarget.StatusEffectHolder.transform.GetComponent<IHealth>().Damage(healthEffectValue);
        }
        else
        {
            statusEffectTarget.StatusEffectHolder.transform.GetComponent<IHealth>().Heal(healthEffectValue);
        }
    }
}