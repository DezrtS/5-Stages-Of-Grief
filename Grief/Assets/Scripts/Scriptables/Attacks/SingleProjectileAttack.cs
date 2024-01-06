using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Attacks/Ranged Attacks/Single Projectile Attack")]
public class SingleProjectileAttack : RangedAttack
{
    [Space(10)]
    [Header("Single Projectile Attack Variables")]
    [SerializeField] private ProjectileData projectileData;
    [SerializeField] private Vector3 spawnOffset;
    [SerializeField] private bool fireProjectileOnAttack = true;

    private GameObject projectile;

    public override Attack Clone(Attack clone, IAttack attacker, Transform parentTransform)
    {
        if (clone == null)
        {
            clone = CreateInstance(GetType()) as Attack;
        }

        SingleProjectileAttack newClone = clone as SingleProjectileAttack;

        newClone.playAudioIdOnAim = playAudioIdOnAim;
        newClone.playAudioIdOnAttack = playAudioIdOnAttack;
        newClone.playAudioIdOnCancel = playAudioIdOnCancel;

        newClone.particleEffectOnAimPrefab = particleEffectOnAimPrefab;
        newClone.particleEffectOnAttackPrefab = particleEffectOnAttackPrefab;
        newClone.particleEffectOnCancelPrefab = particleEffectOnCancelPrefab;

        newClone.projectileData = projectileData;
        newClone.spawnOffset = spawnOffset;

        return base.Clone(newClone, attacker, parentTransform);
    }

    public override void OnAttackStateStart(AttackState attackState)
    {
        base.OnAttackStateStart(attackState);

        if (attackState == AttackState.Aiming)
        {
            projectile = SpawnProjectile(projectileData, parentTransform.position + parentTransform.rotation * spawnOffset, parentTransform.forward);
            PlayAudio(playAudioIdOnAim);

            if (particleEffectOnAimPrefab != null)
            {
                if (particleEffectOnAim != null)
                {
                    particleEffectOnAim.Play();
                }
                else
                {
                    particleEffectOnAim = attacker.AttackHolder.AddParticleEffect(particleEffectOnAimPrefab, spawnOffset, 1);
                    particleEffectOnAim.Play();
                }
            }
        }

        if (attackState == AttackState.Attacking && fireProjectileOnAttack)
        {
            FireProjectile(projectileData, projectile, parentTransform.forward);
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
                    particleEffectOnAttack = attacker.AttackHolder.AddParticleEffect(particleEffectOnAttackPrefab, spawnOffset, 1);
                    particleEffectOnAttack.Play();
                }
            }
        }
    }

    public override void OnAttackState(AttackState attackState, float timeSinceStateStart)
    {
        if (attackState == AttackState.Aiming)
        {
            projectile.transform.position = parentTransform.position + parentTransform.rotation * spawnOffset;
            projectile.transform.forward = parentTransform.forward;
        }

        base.OnAttackState(attackState, timeSinceStateStart);
    }

    public override void OnAttackStateCancel(AttackState attackState, bool otherHasCancelled)
    {
        base.OnAttackStateCancel(attackState, otherHasCancelled);

        if (attackState == AttackState.Aiming)
        {
            Destroy(projectile);
            PlayAudio(playAudioIdOnCancel);
        }

        if (particleEffectOnAim != null)
        {
            particleEffectOnAim.Stop();
        }
        if (particleEffectOnAttack != null)
        {
            particleEffectOnAttack.Stop();
        }

        if (particleEffectOnCancelPrefab != null)
        {
            if (particleEffectOnCancel != null)
            {
                particleEffectOnCancel.Play();
            }
            else
            {
                particleEffectOnCancel = attacker.AttackHolder.AddParticleEffect(particleEffectOnCancelPrefab, spawnOffset, 1);
                particleEffectOnCancel.Play();
            }
        }
    }
}