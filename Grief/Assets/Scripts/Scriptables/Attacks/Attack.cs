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
    [SerializeField] private float attackChargeUpTime;

    [SerializeField] private bool hasAimingStage = true;
    [SerializeField] private bool hasAttackStage = true;

    [SerializeField] private float maxAimTime;
    [SerializeField] private float maxAttackTime;

    private float timeStateStarted = 0;

    private float timeAimingStateEnded = 0;
    private float timeAttackingStateEnded = 0;

    private float timeSinceAimStart = 0;
    private float timeSinceLastAttack = 0;

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
        clone.attackChargeUpTime = attackChargeUpTime;

        clone.isClone = true;

        return clone;
    }

    public virtual void DestroyClone()
    {
        Destroy(this);
    }

    public virtual bool TransferToAttackState<T>(AttackState attackState, T attacker, Transform attackingTransform) where T : IAttack
    {
        if (CanInitiateAttackState(attackState))
        {
            OnAttackStateEnd(this.attackState, attacker);

            this.attackState = attackState;
            InitiateAttackState(attackState, attacker, attackingTransform);

            return true;
        }

        return false;
    }

    public virtual void InitiateAttackState<T>(AttackState attackState, T attacker, Transform attackingTransform) where T : IAttack
    {
        attacker.TransferToAttackState(attackState);
        
        if (attackState == AttackState.Idle)
        {
            return;
        }

        parentTransform = attackingTransform;

        OnAttackStateStart(attackState, attacker);

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
            return (Time.timeSinceLevelLoad - timeSinceAimStart >= attackChargeUpTime);
        } 
        else if (attackState == AttackState.Aiming)
        {
            return (Time.timeSinceLevelLoad - timeSinceLastAttack >= attackCooldown || timeSinceLastAttack == 0);
        }

        return true;
    }

    public virtual void OnAttackStateStart<T>(AttackState attackState, T attacker) where T : IAttack
    {
        attacker.OnAttackStateStart(attackState);
    }

    public virtual void OnAttackState<T>(AttackState attackState, T attacker, float timeSinceStateStarted) where T : IAttack
    {
        attacker.OnAttackState(attackState);

        // Check if attack or aiming has lasted until its max
    }

    public virtual void OnAttackStateEnd<T>(AttackState attackState, T attacker) where T : IAttack
    {
        attacker.OnAttackStateEnd(attackState);
        CoroutineRunner.Instance.StopCoroutine(attackStateCoroutine);
        parentTransform = null;

        if (attackState == AttackState.Aiming)
        {
            timeAimingStateEnded = Time.timeSinceLevelLoad;
        } 
        else if (attackState == AttackState.Attacking)
        {
            timeAttackingStateEnded = Time.timeSinceLevelLoad;
        }
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

        if (attackState == AttackState.Aiming)
        {
            timeAimingStateEnded = Time.timeSinceLevelLoad;
        }
        else if (attackState == AttackState.Attacking)
        {
            timeAttackingStateEnded = Time.timeSinceLevelLoad;
        }
    }

    public IEnumerator AttackStateCoroutine<T>(AttackState attackState, T attacker) where T : IAttack 
    {
        timeStateStarted = Time.timeSinceLevelLoad;

        while (true)
        {
            yield return new WaitForFixedUpdate();
            OnAttackState(attackState, attacker, Time.timeSinceLevelLoad - timeStateStarted);
        }
    }
}