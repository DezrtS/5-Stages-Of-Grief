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
    [SerializeField] protected string playAudioIdOnAim;
    [SerializeField] protected string playAudioIdOnAttack;
    [SerializeField] protected string playAudioIdOnCooldown;
    [SerializeField] protected string playAudioIdOnCancel;

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

        return base.Clone(newClone, attacker, parentTransform);
    }

    public override float GetDamage()
    {
        return damage;
    }
}