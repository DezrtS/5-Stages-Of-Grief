using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackEffectType
{
    Burning,
    Frozen,
    Poisoned,
    Regenerating,
    Stunned
}

public abstract class Attack : ScriptableObject
{
    [Header("Attack Variables")]

    [SerializeField] private string attackId;
    [SerializeField] private float damage = 3;
    [SerializeField] private float knockbackPower;

    [SerializeField] private List<AttackEffectType> attackEffects = new List<AttackEffectType>();
    
    [SerializeField] private float attackCooldown;
    [SerializeField] private float attackCancelCooldown;
    [SerializeField] private float attackChargeUpTime;

    [SerializeField] private bool hasAimingStage = true;
    [SerializeField] private bool hasAttackStage = true;

    [SerializeField] private float maxAimTime = 99;
    [SerializeField] private float maxAttackTime = 99;

    private float timeAimingStateStarted = int.MinValue;
    private float timeAttackingStateEnded = int.MinValue;

    private bool isClone = false;

    protected IAttack attacker;
    protected Transform parentTransform;
    private IEnumerator attackStateCoroutine;

    private AttackState attackState = AttackState.Idle;

    public string AttackId { get { return attackId; } }
    public float Damage { get { return damage; } }
    public float KnockbackPower { get { return knockbackPower; } }
    public AttackState AttackState { get { return attackState; } }

    public virtual Attack Clone(Attack clone, IAttack attacker, Transform parentTransform)
    {
        if (clone == null)
        {
            clone = CreateInstance(GetType()) as Attack;
        }

        clone.attackId = attackId;
        clone.damage = damage;
        clone.knockbackPower = knockbackPower;

        clone.attackEffects = attackEffects;
        clone.attackCooldown = attackCooldown;
        clone.attackCancelCooldown = attackCancelCooldown;
        clone.attackChargeUpTime = attackChargeUpTime;
        clone.hasAimingStage = hasAimingStage;
        clone.hasAttackStage = hasAttackStage;
        clone.maxAimTime = maxAimTime;
        clone.maxAttackTime = maxAttackTime;

        clone.attacker = attacker;
        clone.parentTransform = parentTransform;

        clone.isClone = true;

        return clone;
    }

    public virtual void DestroyClone()
    {
        Destroy(this);
    }

    public virtual void TransferToAttackState(AttackState attackState)
    {
        if (CanInitiateAttackState(attackState))
        {
            OnAttackStateEnd(this.attackState);

            if (!hasAimingStage && attackState == AttackState.Aiming)
            {
                attacker.InitiateAttackState(AttackState.Attacking);
                return;
            } 
            else if (!hasAttackStage && attackState == AttackState.Attacking)
            {
                attackState = AttackState.Idle;
            }

            this.attackState = attackState;
            InitiateAttackState(attackState);
        } 
        else
        {
            if (this.attackState == AttackState.Aiming && attackState == AttackState.Attacking)
            {
                OnAttackStateCancel(this.attackState, false);
            }
        }
    }

    public virtual void InitiateAttackState(AttackState attackState)
    {
        attacker.TransferToAttackState(attackState);

        OnAttackStateStart(attackState);

        if (attackState == AttackState.Idle)
        {
            return;
        }

        attackStateCoroutine = AttackStateCoroutine(attackState);
        CoroutineRunner.Instance.StartCoroutine(attackStateCoroutine);
    }

    public virtual bool CanInitiateAttackState(AttackState attackState)
    {
        if (!isClone)
        {
            return false;
        }

        if (attackState == AttackState.Attacking)
        {
            return (Time.timeSinceLevelLoad - timeAimingStateStarted >= attackChargeUpTime);
        } 
        else if (attackState == AttackState.Aiming)
        {
            return (Time.timeSinceLevelLoad - timeAttackingStateEnded >= attackCooldown);
        }

        return true;
    }

    public virtual void OnAttackStateStart(AttackState attackState)
    {
        attacker.OnAttackStateStart(attackState);

        if (attackState == AttackState.Aiming)
        {
            timeAimingStateStarted = Time.timeSinceLevelLoad;
        }
    }

    public virtual void OnAttackState(AttackState attackState, float timeSinceStateStarted)
    {
        attacker.OnAttackState(attackState);

        if (attackState == AttackState.Aiming && maxAimTime < timeSinceStateStarted)
        {
            TransferToAttackState(AttackState.Attacking);
        } 
        else if (attackState == AttackState.Attacking && maxAttackTime < timeSinceStateStarted)
        {
            OnAttackStateEnd(attackState);
        }
    }

    public virtual void OnAttackStateEnd(AttackState attackState)
    {
        attacker.OnAttackStateEnd(attackState);

        this.attackState = AttackState.Idle;
        CoroutineRunner.Instance.StopCoroutine(attackStateCoroutine);

        if (attackState == AttackState.Attacking)
        {
            timeAttackingStateEnded = Time.timeSinceLevelLoad;
        }

        attacker.TransferToAttackState(AttackState.Idle);
    }

    public virtual void OnAttackStateCancel(AttackState attackState, bool otherHasCancelled)
    {
        if (!otherHasCancelled)
        {
            attacker.OnAttackStateCancel(attackState, true);
        }

        this.attackState = AttackState.Idle;
        CoroutineRunner.Instance.StopCoroutine(attackStateCoroutine);

        if (attackState == AttackState.Attacking)
        {
            timeAttackingStateEnded = Time.timeSinceLevelLoad - attackCooldown + attackCancelCooldown;
        }
    }

    public IEnumerator AttackStateCoroutine(AttackState attackState) 
    {
        float timeStateStarted = Time.timeSinceLevelLoad;

        while (true)
        {
            yield return new WaitForFixedUpdate();
            OnAttackState(attackState, Time.timeSinceLevelLoad - timeStateStarted);
        }
    }

    public virtual void OnAttackTriggerEnter(IHealth entity, Transform entityTransform)
    {
        CombatManager.Instance.DamageEntity(this, attacker, entity);
        CombatManager.Instance.ApplyKnockback(this, parentTransform, entityTransform);
    }

    public virtual void OnAttackTriggerStay(IHealth entity)
    {

    }

    public virtual void OnAttackTriggerExit(IHealth entity)
    {

    }
}