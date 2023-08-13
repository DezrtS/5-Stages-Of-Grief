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
    // Shared Variables
    [SerializeField] private List<AttackEffectType> attackEffects = new List<AttackEffectType>();
    
    [SerializeField] private float attackCooldown;
    [SerializeField] private float attackChargeUpTime;

    [SerializeField] private bool hasAimingStage;
    [SerializeField] private bool hasAttackStage;

    [SerializeField] private float maxAimTime;
    [SerializeField] private float maxAttackTime;

    private float timeWhenStateStarted = 0;
    private float timeWhenStateEnded = 0;

    private float timeSinceAimStart = 0;
    private float timeSinceLastAttack = 0;

    private bool isClone = false;

    protected Transform parentTransform;
    private IEnumerator stateCoroutine;

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

    public virtual bool TransferToState<T>(AttackState attackState, T attacker, Transform attackingTransform) where T : IAttack
    {
        if (CanInitiateState(attackState))
        {
            OnStateEnd(this.attackState, attacker);

            //Debug.Log($"Attack transfering from {this.attackState} to {attackState}");

            this.attackState = attackState;
            InitiateState(attackState, attacker, attackingTransform);

            return true;
        }

        return false;
    }

    public virtual void InitiateState<T>(AttackState attackState, T attacker, Transform attackingTransform) where T : IAttack
    {
        attacker.TransferToState(attackState);
        
        if (attackState == AttackState.Idle)
        {
            return;
        }

        parentTransform = attackingTransform;

        OnStateStart(attackState, attacker);

        stateCoroutine = StateCoroutine(attackState, attacker);
        CoroutineRunner.Instance.StartCoroutine(stateCoroutine);
    }

    public virtual bool CanInitiateState(AttackState attackState)
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

    public virtual void OnStateStart<T>(AttackState attackState, T attacker) where T : IAttack
    {
        attacker.OnStateStart(attackState);
    }

    public virtual void OnState<T>(AttackState attackState, T attacker, float timeSinceStateStart) where T : IAttack
    {
        attacker.OnState(attackState);
    }

    public virtual void OnStateEnd<T>(AttackState attackState, T attacker) where T : IAttack
    {
        attacker.OnStateEnd(attackState);
        CoroutineRunner.Instance.StopCoroutine(stateCoroutine);
        parentTransform = null;
        timeWhenStateEnded = Time.timeSinceLevelLoad;
    }

    public virtual void OnStateCancel<T>(AttackState attackState, T attacker) where T : IAttack
    {
        attackState = AttackState.Idle;
        CoroutineRunner.Instance.StopCoroutine(stateCoroutine);
        parentTransform = null;
        timeWhenStateEnded = Time.timeSinceLevelLoad;
    }

    public IEnumerator StateCoroutine<T>(AttackState attackState, T attacker) where T : IAttack 
    {
        timeWhenStateStarted = Time.timeSinceLevelLoad;

        while (true)
        {
            yield return new WaitForFixedUpdate();
            OnState(attackState, attacker, Time.timeSinceLevelLoad - timeWhenStateStarted);
            
            // Check if attack or aiming has lasted until its max
        }
    }
}