using FMODUnity;
using UnityEngine;

public abstract class PhysicalAttack : Attack
{
    [Space(15)]
    [Header("Physical Attack Variables")]
    [SerializeField] private float damage = 3;
    [SerializeField] private float knockbackPower;
    [SerializeField] protected float attackTriggerScale = 3;
    [SerializeField] protected float attackTriggerSpawnDistance = 2;

    [Space(15)]
    [Header("Audio Variables")]
    [field: SerializeField] protected EventReference playAudioIdOnAim;
    [field: SerializeField] protected EventReference playAudioIdOnAttack;
    [field: SerializeField] protected EventReference playAudioIdOnCooldown;
    [field: SerializeField] protected EventReference playAudioIdOnCancel;

    [Space(15)]
    [Header("Particle Effect Variables")]
    [SerializeField] protected GameObject particleEffectOnAimPrefab;
    [SerializeField] protected GameObject particleEffectOnAttackPrefab;
    [SerializeField] protected GameObject particleEffectOnCooldownPrefab;
    [SerializeField] protected GameObject particleEffectOnCancelPrefab;

    protected ParticleSystem particleEffectOnAim;
    protected ParticleSystem particleEffectOnAttack;
    protected ParticleSystem particleEffectOnCooldown;
    protected ParticleSystem particleEffectOnCancel;

    public override Attack Clone(Attack clone, IAttack attacker, Transform parentTransform)
    {
        if (clone == null)
        {
            clone = CreateInstance(GetType()) as Attack;
        }

        PhysicalAttack newClone = clone as PhysicalAttack;

        newClone.damage = damage;
        newClone.knockbackPower = knockbackPower;
        newClone.attackTriggerScale = attackTriggerScale;
        newClone.attackTriggerSpawnDistance = attackTriggerSpawnDistance;

        newClone.playAudioIdOnAim = playAudioIdOnAim;
        newClone.playAudioIdOnAttack = playAudioIdOnAttack;
        newClone.playAudioIdOnCooldown = playAudioIdOnCooldown;
        newClone.playAudioIdOnCancel = playAudioIdOnCancel;

        newClone.particleEffectOnAimPrefab = particleEffectOnAimPrefab;
        newClone.particleEffectOnAttackPrefab = particleEffectOnAttackPrefab;
        newClone.particleEffectOnCooldownPrefab = particleEffectOnCooldownPrefab;
        newClone.particleEffectOnCancelPrefab = particleEffectOnCancelPrefab;

        return base.Clone(newClone, attacker, parentTransform);
    }

    public override void OnAttackTriggerEnter(Transform hit)
    {
        OnAttackHit(hit, damage, knockbackPower);
    }
}