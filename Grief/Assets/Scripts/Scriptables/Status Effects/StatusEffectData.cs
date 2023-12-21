using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

public enum StatusEffectType
{
    Fire,
    Ice,
    Poison,
    Bleed,
    Regen,
    Stun,
    Visual
}

public abstract class StatusEffectData : ScriptableObject
{
    [Header("Status Effect Variables")]
    [SerializeField] private string statusEffectId;
    [SerializeField] private StatusEffectType statusEffectType;

    [SerializeField] private float statusEffectDuration = 0f;
    [SerializeField] private float timeBetweenEffectActivations = 99f;

    [SerializeField] private bool applyEffectOnStart = true;
    [SerializeField] private bool applyEffectOnEnd = false;

    [SerializeField] private GameObject particleEffectOnStartPrefab;
    [SerializeField] private GameObject particleEffectOnApplyPrefab;

    [field: SerializeField] private EventReference playAudioOnStart;
    [field: SerializeField] private EventReference playAudioOnApply;

    public string StatusEffectId { get { return statusEffectId; } }
    public StatusEffectType StatusEffectType { get { return statusEffectType; } }
    public float StatusEffectDuration { get { return statusEffectDuration; } }
    public float TimeBetweenEffectActivations { get { return timeBetweenEffectActivations; } }
    public bool ApplyEffectOnStart { get { return applyEffectOnStart; } }
    public bool ApplyEffectOnEnd { get { return applyEffectOnEnd; } }
    public EventReference PlayAudioOnStart {  get { return playAudioOnStart; } }
    public EventReference PlayAudioOnApply {  get { return playAudioOnApply; } }

    public abstract bool CanApplyStatusEffect(IStatusEffectTarget statusEffectTarget);

    public abstract void ApplyStatusEffect(IStatusEffectTarget statusEffectTarget);

    public virtual void ApplyStatusEffectParticleEffects(IStatusEffectTarget statusEffectTarget, List<ParticleSystem> particleEffectsOnStart, List<ParticleSystem> particleEffectsOnApply)
    {
        if (particleEffectOnStartPrefab != null)
        {
            GameObject particleOnStart = Instantiate(particleEffectOnStartPrefab, statusEffectTarget.StatusEffectHolder.ParticleEffectHolder.transform);
            particleEffectsOnStart.Add(particleOnStart.GetComponent<ParticleSystem>());
        }

        if (particleEffectOnApplyPrefab != null)
        {
            GameObject particleOnApply = Instantiate(particleEffectOnApplyPrefab, statusEffectTarget.StatusEffectHolder.ParticleEffectHolder.transform);
            particleEffectsOnApply.Add(particleOnApply.GetComponent<ParticleSystem>());
        }
    }

    public virtual void ClearStatusEffect(IStatusEffectTarget statusEffectTarget) { }
}