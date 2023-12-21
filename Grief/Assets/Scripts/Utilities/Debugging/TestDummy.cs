using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TestDummy : Enemy, IStatusEffectTarget
{
    private StatusEffectHolder statusEffectHolder;

    public StatusEffectHolder StatusEffectHolder {  get { return statusEffectHolder; } }

    protected override void Awake()
    {
        base.Awake();

        if (!TryGetComponent(out statusEffectHolder))
        {
            statusEffectHolder = transform.AddComponent<StatusEffectHolder>();
        }
    }

    public void ApplyHealthStatusEffect(float healthEffectValue, bool isDamage)
    {
        if (isDamage)
        {
            Damage(healthEffectValue);
        }
        else
        {
            Heal(healthEffectValue);
        }
    }
}