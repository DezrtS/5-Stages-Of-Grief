using UnityEngine;

public class PhysicalAttack : Attack
{
    [Space(10)]
    [Header("Physical Attack Variables")]
    [SerializeField] protected float attackTriggerScale = 3;
    [SerializeField] protected float attackTriggerSpawnDistance = 2;

    public override Attack Clone(Attack clone, IAttack attacker, Transform parentTransform)
    {
        if (clone == null)
        {
            clone = CreateInstance(GetType()) as Attack;
        }

        PhysicalAttack newClone = clone as PhysicalAttack;

        newClone.attackTriggerScale = attackTriggerScale;
        newClone.attackTriggerSpawnDistance = attackTriggerSpawnDistance;

        return base.Clone(newClone, attacker, parentTransform);
    }
}