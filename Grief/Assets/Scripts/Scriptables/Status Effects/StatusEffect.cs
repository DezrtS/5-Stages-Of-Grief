using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatusEffectType
{
    Burning,
    Freezing,
    Poisoning,
    Bleeding,
    Regenerating,
    Stunning
}

public abstract class StatusEffect : ScriptableObject
{
    [SerializeField] private string statusEffectId;
    [SerializeField] private StatusEffectType statusEffectType;

    [SerializeField] private float duration = 0f;
    [SerializeField] protected float timeBetweenEffectActivations = 99f;

    [SerializeField] private bool applyEffectOnStart = true;
    [SerializeField] private bool applyEffectOnEnd = false;

    protected float durationRemaining;
    protected float timeAtLastEffectActivation = 0;

    protected IStatusEffectTarget statusEffectTarget;
    protected Transform parentTransform;

    private bool isClone;

    public string StatusEffectId { get { return statusEffectId; } }
    public StatusEffectType StatusEffectType { get { return statusEffectType; } }

    public virtual StatusEffect Clone(StatusEffect clone, IStatusEffectTarget statusEffectTarget, Transform parentTransform)
    {
        if (clone == null)
        {
            clone = CreateInstance(GetType()) as StatusEffect;
        }

        clone.statusEffectId = statusEffectId;
        clone.statusEffectType = statusEffectType;

        clone.duration = duration;
        clone.timeBetweenEffectActivations = timeBetweenEffectActivations;
        clone.applyEffectOnStart = applyEffectOnStart;
        clone.applyEffectOnEnd = applyEffectOnEnd;

        clone.durationRemaining = duration;
        clone.timeAtLastEffectActivation = Time.timeSinceLevelLoad;

        clone.statusEffectTarget = statusEffectTarget;
        clone.parentTransform = parentTransform;

        clone.isClone = true;

        return clone;
    }

    public virtual void DestroyClone()
    {
        Destroy(this);
    }

    public virtual void ApplyEffect()
    {
        // Area to Apply Effect in Child Classes
    }

    public virtual bool CanAffectTarget(IStatusEffectTarget statusEffectTarget, Transform parentTransform)
    {
        if (!isClone)
        {
            return false;
        }



        return true;
    }

    public virtual void UpdateEffect()
    {
        if (applyEffectOnStart)
        {
            ApplyEffect();
            applyEffectOnStart = false;
        }

        if (durationRemaining <= 0)
        {
            durationRemaining -= Time.deltaTime;

            if (Time.timeSinceLevelLoad - timeAtLastEffectActivation >= timeBetweenEffectActivations)
            {
                timeAtLastEffectActivation = Time.timeSinceLevelLoad;
                ApplyEffect();
            }
        } 
        else
        {
            if (applyEffectOnEnd)
            {
                ApplyEffect();
                applyEffectOnEnd = false;
            }

            RemoveEffect();
        }
    }

    public virtual void RemoveEffect()
    {
        if (isClone)
        {
            Debug.LogWarning($"Trying To Destroy StatusEffect {statusEffectId} Template");
            return;
        }

        statusEffectTarget.RemoveStatusEffect(this);
        DestroyClone();
    }

    public void ResetDuration()
    {
        durationRemaining = duration;
    }
}