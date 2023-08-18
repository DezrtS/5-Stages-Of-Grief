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

    protected Transform parentTransform;
    private IEnumerator attackStateCoroutine;

    private AttackState attackState = AttackState.Idle;
    public AttackState AttackState { get { return attackState; } }

    public virtual Attack Clone(Attack clone)
    {
        if (clone == null)
        {
            clone = CreateInstance(GetType()) as Attack;
        }

        clone.attackEffects = attackEffects;
        clone.attackCooldown = attackCooldown;
        clone.attackCancelCooldown = attackCancelCooldown;
        clone.attackChargeUpTime = attackChargeUpTime;
        clone.hasAimingStage = hasAimingStage;
        clone.hasAttackStage = hasAttackStage;
        clone.maxAimTime = maxAimTime;
        clone.maxAttackTime = maxAttackTime;

        clone.isClone = true;

        return clone;
    }

    public virtual void DestroyClone()
    {
        Destroy(this);
    }

    public virtual void TransferToAttackState<T>(AttackState attackState, T attacker, Transform attackingTransform) where T : IAttack
    {
        if (CanInitiateAttackState(attackState))
        {
            OnAttackStateEnd(this.attackState, attacker);

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
            InitiateAttackState(attackState, attacker, attackingTransform);
        } 
        else
        {
            if (this.attackState == AttackState.Aiming && attackState == AttackState.Attacking)
            {
                OnAttackStateCancel(this.attackState, attacker, false);
            }
        }
    }

    public virtual void InitiateAttackState<T>(AttackState attackState, T attacker, Transform attackingTransform) where T : IAttack
    {
        attacker.TransferToAttackState(attackState);

        parentTransform = attackingTransform;

        OnAttackStateStart(attackState, attacker);

        if (attackState == AttackState.Idle)
        {
            return;
        }

        attackStateCoroutine = AttackStateCoroutine(attackState, attacker);
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

    public virtual void OnAttackStateStart<T>(AttackState attackState, T attacker) where T : IAttack
    {
        attacker.OnAttackStateStart(attackState);

        if (attackState == AttackState.Aiming)
        {
            timeAimingStateStarted = Time.timeSinceLevelLoad;
        }
    }

    public virtual void OnAttackState<T>(AttackState attackState, T attacker, float timeSinceStateStarted) where T : IAttack
    {
        attacker.OnAttackState(attackState);

        if (attackState == AttackState.Aiming && maxAimTime < timeSinceStateStarted)
        {
            TransferToAttackState(AttackState.Attacking, attacker, parentTransform);
        } 
        else if (attackState == AttackState.Attacking && maxAttackTime < timeSinceStateStarted)
        {
            OnAttackStateEnd(attackState, attacker);
        }
    }

    public virtual void OnAttackStateEnd<T>(AttackState attackState, T attacker) where T : IAttack
    {
        attacker.OnAttackStateEnd(attackState);

        this.attackState = AttackState.Idle;
        CoroutineRunner.Instance.StopCoroutine(attackStateCoroutine);
        parentTransform = null;

        if (attackState == AttackState.Attacking)
        {
            timeAttackingStateEnded = Time.timeSinceLevelLoad;
        }

        attacker.TransferToAttackState(AttackState.Idle);
    }

    public virtual void OnAttackStateCancel<T>(AttackState attackState, T attacker, bool otherHasCancelled) where T : IAttack
    {
        if (!otherHasCancelled)
        {
            attacker.OnAttackStateCancel(attackState, true);
        }

        this.attackState = AttackState.Idle;
        CoroutineRunner.Instance.StopCoroutine(attackStateCoroutine);
        parentTransform = null;

        if (attackState == AttackState.Attacking)
        {
            timeAttackingStateEnded = Time.timeSinceLevelLoad - attackCooldown + attackCancelCooldown;
        }
    }

    public IEnumerator AttackStateCoroutine<T>(AttackState attackState, T attacker) where T : IAttack 
    {
        float timeStateStarted = Time.timeSinceLevelLoad;

        while (true)
        {
            yield return new WaitForFixedUpdate();
            OnAttackState(attackState, attacker, Time.timeSinceLevelLoad - timeStateStarted);
        }
    }
}