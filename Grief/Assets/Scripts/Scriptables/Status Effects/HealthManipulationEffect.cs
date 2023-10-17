using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Status Effects/Health Manipulation Effect")]
public class HealthManipulationEffect : StatusEffect
{
    [Space(15)]
    [Header("Health Manipulation Effect Variables")]
    [SerializeField] private float healthEffectValue;
    [SerializeField] private bool isDamage = false;

    private IHealth entityHealth;


    public override StatusEffect Clone(StatusEffect clone, IStatusEffectTarget statusEffectTarget, Transform parentTransform)
    {
        if (clone == null)
        {
            clone = CreateInstance(GetType()) as StatusEffect;
        }

        HealthManipulationEffect newClone = clone as HealthManipulationEffect;

        newClone.healthEffectValue = healthEffectValue;
        newClone.isDamage = isDamage;
        newClone.entityHealth = parentTransform.GetComponent<IHealth>();

        return base.Clone(newClone, statusEffectTarget, parentTransform);
    }

    public override void ApplyEffect()
    {
        if (isDamage)
        {
            entityHealth.Damage(healthEffectValue);
        } 
        else
        {
            entityHealth.Heal(healthEffectValue);
        }

        base.ApplyEffect();
    }

    public override bool CanAffectTarget(IStatusEffectTarget statusEffectTarget, Transform parentTransform)
    {
        if (!parentTransform.TryGetComponent<IHealth>(out _))
        {
            return false;
        }

        return base.CanAffectTarget(statusEffectTarget, parentTransform);
    }
}