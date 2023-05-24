using UnityEngine;

public abstract class Attack : ScriptableObject
{
    [SerializeField] private float attackRange;
    [SerializeField] private float attackDamage;
    [SerializeField] private float attackCooldown;

    public float AttackRange { get { return attackRange; } }
    public float AttackDamage { get { return attackDamage; } }
    public float AttackCooldown { get { return attackCooldown; } }

    public virtual void InitiateAttack()
    {

    }
}