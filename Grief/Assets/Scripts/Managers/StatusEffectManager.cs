using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectManager : Singleton<StatusEffectManager>
{
    static private List<IStatusEffectTarget> statusEffectTargets = new List<IStatusEffectTarget>();

    private void FixedUpdate()
    {
        if (statusEffectTargets.Count > 0)
        {
            for (int statusEffectTargetIndex = 0; statusEffectTargetIndex < statusEffectTargets.Count; statusEffectTargetIndex++) 
            {
                IStatusEffectTarget target = statusEffectTargets[statusEffectTargetIndex];

                List<StatusEffect> statusEffects = target.StatusEffects;

                if (target.StatusEffects.Count == 0)
                {
                    Debug.LogWarning("StatusEffectTarget With no StatusEffects is Still Being Called");
                }

                for (int statusEffectIndex = 0; statusEffectIndex < statusEffects.Count; statusEffectIndex++)
                {
                    StatusEffect statusEffect = statusEffects[statusEffectIndex];
                    statusEffect.UpdateEffect();
                    
                    if (statusEffect.IsFinished)
                    {
                        statusEffect.RemoveEffect();
                        statusEffectIndex--;

                        if (target.StatusEffects.Count == 0)
                        {
                            statusEffectTargets.Remove(target);
                            statusEffectTargetIndex--;
                        }
                    }
                }
            }
        }
    }

    static public void AddStatusEffectToObject(StatusEffect statusEffect, IStatusEffectTarget statusEffectTarget, Transform targetTransform)
    {
        if (statusEffect.CanAffectTarget(statusEffectTarget, targetTransform))
        {
            statusEffectTarget.AddStatusEffect(statusEffect.Clone(null, statusEffectTarget, targetTransform));

            if (!statusEffectTargets.Contains(statusEffectTarget))
            {
                statusEffectTargets.Add(statusEffectTarget);
            }
        }
    }

    static public void AddStatusEffectToObject(List<StatusEffect> statusEffects, IStatusEffectTarget statusEffectTarget, Transform targetTransform)
    {
        foreach (StatusEffect effect in statusEffects)
        {
            if (effect.CanAffectTarget(statusEffectTarget, targetTransform))
            {
                statusEffectTarget.AddStatusEffect(effect.Clone(null, statusEffectTarget, targetTransform));

                if (!statusEffectTargets.Contains(statusEffectTarget))
                {
                    statusEffectTargets.Add(statusEffectTarget);
                }
            }
        }
    }

    static public void RemoveStatusEffectFromObject(StatusEffect statusEffect, IStatusEffectTarget statusEffectTarget)
    {
        statusEffect.RemoveEffect();

        if (statusEffectTarget.StatusEffects.Count == 0)
        {
            if (statusEffectTargets.Contains(statusEffectTarget))
            {
                statusEffectTargets.Remove(statusEffectTarget);
            }
        }
    }

    static public void RemoveAllStatusEffectFromObject(IStatusEffectTarget statusEffectTarget)
    {
        List<StatusEffect> statusEffects = statusEffectTarget.StatusEffects;

        for (int statusEffectIndex = 0; statusEffectIndex < statusEffects.Count; statusEffectIndex++)
        {
            StatusEffect statusEffect = statusEffectTarget.StatusEffects[statusEffectIndex];

            if (statusEffects.Contains(statusEffect))
            {
                RemoveStatusEffectFromObject(statusEffect, statusEffectTarget);
                statusEffectIndex--;
            }
        }

        if (statusEffectTargets.Contains(statusEffectTarget))
        {
            statusEffectTargets.Remove(statusEffectTarget);
        }
    }

    static public void RemoveStatusEffectTypeFromObject(StatusEffectType statusEffectType, IStatusEffectTarget statusEffectTarget)
    {
        List<StatusEffect> statusEffects = statusEffectTarget.StatusEffects;

        for (int statusEffectIndex = 0; statusEffectIndex < statusEffects.Count; statusEffectIndex++)
        {
            StatusEffect statusEffect = statusEffects[statusEffectIndex];

            if (statusEffect.StatusEffectType == statusEffectType)
            {
                if (statusEffects.Contains(statusEffect))
                {
                    RemoveStatusEffectFromObject(statusEffect, statusEffectTarget);
                    statusEffectIndex--;
                }
            }
        }

        if (statusEffectTarget.StatusEffects.Count == 0)
        {
            if (statusEffectTargets.Contains(statusEffectTarget))
            {
                statusEffectTargets.Remove(statusEffectTarget);
            }
        }
    }

    static public void RemoveStatusEffectTypeFromObject(List<StatusEffectType> statusEffectTypes, IStatusEffectTarget statusEffectTarget)
    {
        List<StatusEffect> statusEffects = statusEffectTarget.StatusEffects;

        for (int statusEffectIndex = 0; statusEffectIndex < statusEffects.Count; statusEffectIndex++)
        {
            StatusEffect statusEffect = statusEffects[statusEffectIndex];

            if (statusEffectTypes.Contains(statusEffect.StatusEffectType))
            {
                if (statusEffects.Contains(statusEffect))
                {
                    RemoveStatusEffectFromObject(statusEffect, statusEffectTarget);
                    statusEffectIndex--;
                }
            }
        }

        if (statusEffectTarget.StatusEffects.Count == 0)
        {
            if (statusEffectTargets.Contains(statusEffectTarget))
            {
                statusEffectTargets.Remove(statusEffectTarget);
            }
        }
    }
}