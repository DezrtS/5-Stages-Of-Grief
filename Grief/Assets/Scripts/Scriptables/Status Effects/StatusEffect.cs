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
    [Header("Status Effect Variables")]
    [SerializeField] private string statusEffectId;
    [SerializeField] private StatusEffectType statusEffectType;

    [SerializeField] private float duration = 0f;
    [SerializeField] protected float timeBetweenEffectActivations = 99f;

    [SerializeField] private bool stunEntity = false;

    [SerializeField] private bool applyEffectOnStart = true;
    [SerializeField] private bool applyEffectOnEnd = false;

    [SerializeField] private GameObject particleEffectOnEffectStart;
    [SerializeField] private GameObject particleEffectOnEffectApply;

    protected float durationRemaining;
    protected float timeAtLastEffectActivation = 0;

    protected IStatusEffectTarget statusEffectTarget;
    protected Transform parentTransform;

    private GameObject particleEffectHolder;

    private ParticleSystem particleEffectOnStart;
    private ParticleSystem particleEffectOnApply;

    private bool isClone;
    private bool isFinished;

    public string StatusEffectId { get { return statusEffectId; } }
    public StatusEffectType StatusEffectType { get { return statusEffectType; } }
    public bool IsFinished { get { return isFinished; } }

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
        clone.stunEntity = stunEntity;
        clone.applyEffectOnStart = applyEffectOnStart;
        clone.applyEffectOnEnd = applyEffectOnEnd;

        if (stunEntity)
        {

        }

        clone.particleEffectOnEffectStart = particleEffectOnEffectStart;
        clone.particleEffectOnEffectApply = particleEffectOnEffectApply;

        clone.durationRemaining = clone.duration;
        clone.timeAtLastEffectActivation = Time.timeSinceLevelLoad;

        clone.statusEffectTarget = statusEffectTarget;
        clone.parentTransform = parentTransform;

        clone.particleEffectHolder = new GameObject("ParticleEffectHolder");
        clone.particleEffectHolder.transform.position = parentTransform.position;

        if (clone.particleEffectOnEffectStart != null)
        {
            clone.particleEffectOnStart = Instantiate(clone.particleEffectOnEffectStart, clone.particleEffectHolder.transform).GetComponent<ParticleSystem>();
            clone.particleEffectOnStart.Play();
        }

        if (clone.particleEffectOnEffectApply != null)
        {
            clone.particleEffectOnApply = Instantiate(clone.particleEffectOnEffectApply, clone.particleEffectHolder.transform).GetComponent<ParticleSystem>();
        }

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

        if (particleEffectOnApply != null)
        {
            particleEffectOnApply.Play();
        }
    }

    public virtual bool CanAffectTarget(IStatusEffectTarget statusEffectTarget, Transform parentTransform)
    {
        return true;
    }

    public virtual void UpdateEffect()
    {
        if (isFinished)
        {
            return;
        }

        particleEffectHolder.transform.position = parentTransform.position;

        if (applyEffectOnStart)
        {
            ApplyEffect();
            applyEffectOnStart = false;
        }

        if (durationRemaining > 0)
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

            isFinished = true;
            //RemoveEffect();
        }
    }

    public virtual void RemoveEffect()
    {
        if (!isClone)
        {
            Debug.LogWarning($"Trying To Destroy StatusEffect {statusEffectId} Template");
            return;
        }

        if (particleEffectOnStart != null)
        {
            particleEffectOnStart.Stop();
        }

        Destroy(particleEffectHolder, 1f);

        statusEffectTarget.RemoveStatusEffect(this);
        DestroyClone();
    }

    public void ResetDuration()
    {
        durationRemaining = duration;
    }
}