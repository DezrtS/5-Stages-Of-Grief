using FMOD.Studio;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectHolder : MonoBehaviour
{
    private IStatusEffectTarget statusEffectTarget;
    private readonly List<StatusEffect> statusEffects = new();

    public IStatusEffectTarget StatusEffectTarget {  get { return statusEffectTarget; } }

    private void Awake()
    {
        if (!TryGetComponent(out statusEffectTarget))
        {
            // Parent Object Does not Support Status Effects
            Destroy(this);
            return;
        }
    }

    private void FixedUpdate()
    {
        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            statusEffects[i].UpdateStatusEffect();
        }
    }

    public bool CanApplyStatusEffect(StatusEffectData statusEffectData)
    {
        foreach (StatusEffect statusEffect in statusEffects)
        {
            if (statusEffect.Data == statusEffectData)
            {
                // Status Effect is Already Applied.
                statusEffect.ResetStatusEffectDuration();
                return false;
            }
        }

        return statusEffectData.CanApplyStatusEffect(statusEffectTarget);
    }
    
    public void AddStatusEffect(StatusEffectData statusEffectData)
    {
        if (CanApplyStatusEffect(statusEffectData))
        {
            statusEffects.Add(new StatusEffect(statusEffectData, this));
        }
    }

    public void AddStatusEffect(List<StatusEffectData> statusEffectDatas)
    {
        foreach(StatusEffectData statusEffectData in statusEffectDatas)
        {
            AddStatusEffect(statusEffectData);
        }
    }

    public void RemoveStatusEffect(StatusEffectData statusEffectData)
    {
        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            if (statusEffects[i].Data == statusEffectData)
            {
                statusEffects.RemoveAt(i);
            }
        }
    }

    public void ClearStatusEffects()
    {
        foreach (StatusEffect statusEffect in statusEffects)
        {
            statusEffect.ClearStatusEffect();
        }

        statusEffects.Clear();
    }

    public void DestroyGameObject(GameObject gameObject, float time)
    {
        Destroy(gameObject, time);
    }
}

public class StatusEffect
{
    private readonly StatusEffectData statusEffectData;
    private readonly StatusEffectHolder statusEffectHolder;
    private float effectDurationRemaining;
    private float timeUntilEffectActivation;

    private readonly List<ParticleSystem> particleEffectsOnStart = new();
    private readonly List<ParticleSystem> particleEffectsOnApply = new();
    private bool appliedOnStart;

    private bool applyEffectOnStart;
    private bool applyEffectOnEnd;

    private EventInstance playAudioOnStart;
    private bool playAudioOnApply;

    public StatusEffectData Data { get { return statusEffectData; } }

    public StatusEffect(StatusEffectData statusEffectData, StatusEffectHolder statusEffectHolder)
    {
        this.statusEffectData = statusEffectData;
        this.statusEffectHolder = statusEffectHolder;
        effectDurationRemaining = statusEffectData.StatusEffectDuration;
        timeUntilEffectActivation = statusEffectData.TimeBetweenEffectActivations;

        statusEffectData.ApplyStatusEffectParticleEffects(statusEffectHolder.StatusEffectTarget, particleEffectsOnStart, particleEffectsOnApply);

        applyEffectOnStart = statusEffectData.ApplyEffectOnStart;
        applyEffectOnEnd = statusEffectData.ApplyEffectOnEnd;

        if (!statusEffectData.PlayAudioOnStart.IsNull)
        {
            playAudioOnStart = AudioManager.Instance.CreateInstance(statusEffectData.PlayAudioOnStart);
            playAudioOnStart.start();
        }

        playAudioOnApply = !statusEffectData.PlayAudioOnApply.IsNull;
    }

    public void UpdateStatusEffect()
    {
        if (applyEffectOnStart)
        {
            applyEffectOnStart = false;
            statusEffectData.ApplyStatusEffect(statusEffectHolder.StatusEffectTarget);
        }

        if (!appliedOnStart)
        {
            appliedOnStart = true;
            foreach (ParticleSystem particleEffect in particleEffectsOnStart)
            {
                particleEffect.Play();
            }
        }

        effectDurationRemaining -= Time.fixedDeltaTime;

        if (effectDurationRemaining <= 0)
        {
            if (applyEffectOnEnd)
            {
                applyEffectOnEnd = false;
                statusEffectData.ApplyStatusEffect(statusEffectHolder.StatusEffectTarget);
            }

            if (!statusEffectData.PlayAudioOnStart.IsNull)
            {
                playAudioOnStart.stop(STOP_MODE.ALLOWFADEOUT);
            }

            ClearStatusEffect();
            statusEffectHolder.RemoveStatusEffect(statusEffectData);
        }
        else
        {
            timeUntilEffectActivation -= Time.fixedDeltaTime;

            if (timeUntilEffectActivation <= 0)
            {
                timeUntilEffectActivation = statusEffectData.TimeBetweenEffectActivations;
                statusEffectData.ApplyStatusEffect(statusEffectHolder.StatusEffectTarget);
                
                foreach (ParticleSystem particleEffect in particleEffectsOnApply)
                {
                    particleEffect.Play();
                }

                if (playAudioOnApply)
                {
                    AudioManager.Instance.PlayOneShot(statusEffectData.PlayAudioOnApply, statusEffectHolder.transform.position);
                }
            }
        }
    }

    public void ClearStatusEffect()
    {
        for (int i = particleEffectsOnStart.Count - 1; i >= 0; i--)
        {
            particleEffectsOnStart[i].Stop();
            statusEffectHolder.DestroyGameObject(particleEffectsOnStart[i].gameObject, 1);
        }

        for (int i = particleEffectsOnApply.Count - 1; i >= 0; i--)
        {
            statusEffectHolder.DestroyGameObject(particleEffectsOnApply[i].gameObject, 1);
        }
    }

    public void ResetStatusEffectDuration()
    {
        effectDurationRemaining = statusEffectData.StatusEffectDuration;
    }
}