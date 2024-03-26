using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Attacks/Physical Attacks/Basic Attack")]
public class BasicAttack : PhysicalAttack
{
    private GameObject attackTrigger;
    private Vector3 triggerOffset;

    public override void OnAttackStateStart(AttackState attackState)
    {
        base.OnAttackStateStart(attackState);

        if (attackState == AttackState.Idle)
        {
            PlayAudio(playAudioIdOnCooldown);

            if (particleEffectOnCooldownPrefab != null)
            {
                if (particleEffectOnCooldown != null)
                {
                    particleEffectOnCooldown.Play();
                }
                else
                {
                    particleEffectOnCooldown = attacker.AttackHolder.AddParticleEffect(particleEffectOnCooldownPrefab, Vector3.forward * attackTriggerSpawnDistance + Vector3.up, attackTriggerScale);
                    particleEffectOnCooldown.Play();
                }
            }
        }
        else if (attackState == AttackState.Aiming)
        {
            PlayAudio(playAudioIdOnAim);

            if (particleEffectOnCancel != null)
            {
                if (particleEffectOnCancel.isPlaying)
                {
                    particleEffectOnCancel.Stop();
                }
            }

            if (particleEffectOnAimPrefab != null)
            {
                if (particleEffectOnAim != null)
                {
                    particleEffectOnAim.Play();
                }
                else
                {
                    particleEffectOnAim = attacker.AttackHolder.AddParticleEffect(particleEffectOnAimPrefab, Vector3.forward * attackTriggerSpawnDistance + Vector3.up, attackTriggerScale);
                    particleEffectOnAim.Play();
                }
            }
        }
        else if (attackState == AttackState.Attacking)
        {
            attackTrigger = CombatManager.Instance.CreateCircleAttackTrigger(this, parentTransform.position + parentTransform.forward * attackTriggerSpawnDistance + Vector3.up, attackTriggerScale);
            triggerOffset = attackTrigger.transform.position - parentTransform.position;
            PlayAudio(playAudioIdOnAttack);

            if (particleEffectOnAim != null)
            {
                if (particleEffectOnAim.isPlaying)
                {
                    particleEffectOnAim.Stop();
                }
            }

            if (particleEffectOnAttackPrefab != null)
            {
                if (particleEffectOnAttack != null)
                {
                    particleEffectOnAttack.Play();
                }
                else
                {
                    particleEffectOnAttack = attacker.AttackHolder.AddParticleEffect(particleEffectOnAttackPrefab, Vector3.forward * attackTriggerSpawnDistance + Vector3.up, attackTriggerScale);
                    particleEffectOnAttack.Play();
                }
            }
        }
    }

    public override void OnAttackState()
    {
        if (attackState == AttackState.Attacking)
        {
            attackTrigger.transform.position = parentTransform.position + triggerOffset;
        }

        base.OnAttackState();
    }

    public override void OnAttackStateEnd(AttackState attackState)
    {
        base.OnAttackStateEnd(attackState);

        if (attackState == AttackState.Attacking)
        {
            Destroy(attackTrigger);
        }
    }

    public override void OnAttackStateCancel(AttackState attackState, bool otherHasCancelled)
    {
        base.OnAttackStateCancel(attackState, otherHasCancelled);

        if (attackState == AttackState.Attacking)
        {
            Destroy(attackTrigger);
        }

        PlayAudio(playAudioIdOnCancel);

        if (particleEffectOnAim != null)
        {
            particleEffectOnAim.Stop();
        }
        if (particleEffectOnAttack != null)
        {
            particleEffectOnAttack.Stop();
        }
        if (particleEffectOnCooldown != null)
        {
            particleEffectOnCooldown.Stop();
        }

        if (particleEffectOnCancelPrefab != null)
        {
            if (particleEffectOnCancel != null)
            {
                particleEffectOnCancel.Play();
            }
            else
            {
                particleEffectOnCancel = attacker.AttackHolder.AddParticleEffect(particleEffectOnCancelPrefab, Vector3.forward * attackTriggerSpawnDistance + Vector3.up, attackTriggerScale);
                particleEffectOnCancel.Play();
            }
        }
    }
}