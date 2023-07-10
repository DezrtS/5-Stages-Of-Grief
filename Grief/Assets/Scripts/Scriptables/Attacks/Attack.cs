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

public enum AttackCommand
{
    AttackButtonPressed,
    AttackButtonReleased,
    AttackAnimationFinished
}

public enum AttackState
{
    Idle,
    Aiming,
    Attacking
}

public abstract class Attack : ScriptableObject
{
    // Shared Variables
    [SerializeField] private List<AttackEffectType> attackEffects = new List<AttackEffectType>();
    [SerializeField] private float attackCooldown;
    [SerializeField] private float attackChargeUpTime;

    [SerializeField] private bool hasMaxAttackLength;
    [SerializeField] private float maxAttacklength;

    private float timeSinceAimStart = 0;
    private float timeSinceLastAttack = 0;
    private bool isClone = false;

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
        clone.hasMaxAttackLength = hasMaxAttackLength;
        clone.maxAttacklength = maxAttacklength;
        clone.isClone = true;

        return clone;
    }

    public virtual void DestroyClone()
    {
        Destroy(this);
    }

    public virtual bool SendCommand(AttackCommand attackCommand, Transform transform)
    {
        if (attackCommand == AttackCommand.AttackButtonPressed)
        {
            if (CanAim())
            {
                OnAimStart(transform);
            }
            else
            {
                return false;
            }
        }
        else if (attackCommand == AttackCommand.AttackButtonReleased)
        {
            if (CanAttack())
            {
                OnAimToAttack(transform);
            }
            else
            {
                CancelAiming();
                return false;
            }
        } 
        else if (attackCommand == AttackCommand.AttackAnimationFinished)
        {
            if (attackState == AttackState.Attacking)
            {
                OnAttackEnd(transform);
            }
        }

        //Debug.Log(attackCommand + " command was sent");

        return true;
    }

    public virtual void OnAimStart(Transform transform)
    {
        //Debug.Log("Aiming Started");
        timeSinceAimStart = Time.timeSinceLevelLoad;
        attackState = AttackState.Aiming;
    }

    public virtual void OnAim(Transform transform)
    {

    }

    public virtual void OnAimToAttack(Transform transform)
    {
        //Debug.Log("Transitioning From Aiming to Attacking");
        attackState = AttackState.Attacking;

        if (hasMaxAttackLength)
        {
            CoroutineRunner.Instance.StartCoroutine(StopAttack(transform));
        }
    }

    public virtual void OnAttack(Transform transform)
    {

    }

    public virtual void OnAttackEnd(Transform transform)
    {
        //Debug.Log("Attack Ended");
        timeSinceLastAttack = Time.timeSinceLevelLoad;
        CoroutineRunner.Instance.StopCoroutine(StopAttack(transform));
        attackState = AttackState.Idle;
    }

    public virtual void CancelAiming()
    {
        //Debug.Log("Aiming Canceled");
        attackState = AttackState.Idle;
    }

    public virtual bool CanAim()
    {
        return (Time.timeSinceLevelLoad - timeSinceLastAttack >= attackCooldown || timeSinceLastAttack == 0) && isClone && attackState == AttackState.Idle;
    }

    public virtual bool CanAttack()
    {
        return (Time.timeSinceLevelLoad - timeSinceAimStart >= attackChargeUpTime);
    }

    public virtual IEnumerator StopAttack(Transform transform)
    {
        yield return new WaitForSeconds(maxAttacklength);
        OnAttackEnd(transform);
    }
}