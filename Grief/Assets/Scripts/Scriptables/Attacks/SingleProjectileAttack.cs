using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Attacks/Ranged Attacks/Single Projectile Attack")]
public class SingleProjectileAttack : RangedAttack
{
    [Space(10)]
    [Header("Single Projectile Attack Variables")]
    [SerializeField] private Projectile projectileData;
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

        newClone.projectileData = projectileData;
        newClone.spawnOffset = spawnOffset;

        return base.Clone(newClone, attacker, parentTransform);
    }

    public override float GetDamage()
    {
        return projectileData.Damage;
    }

    public override void OnAttackStateStart(AttackState attackState)
    {
        base.OnAttackStateStart(attackState);

        if (attackState == AttackState.Aiming)
        {
            projectile = SpawnProjectile(projectileData, parentTransform.position + parentTransform.rotation * spawnOffset, parentTransform.forward);
            PlayAudio(playAudioIdOnAim);
        }

        if (attackState == AttackState.Attacking && fireProjectileOnAttack)
        {
            FireProjectile(projectileData, projectile, parentTransform.forward);
            PlayAudio(playAudioIdOnAttack);
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
    }
}