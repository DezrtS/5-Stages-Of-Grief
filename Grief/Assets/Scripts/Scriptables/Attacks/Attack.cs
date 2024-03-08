using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Attack : ScriptableObject
{
    [Header("Attack Variables")]
    [SerializeField] private string attackId;
    [SerializeField] private float recommendedAttackRange = 2;
    [SerializeField] private float attackRangeDeviation = 1;

    [Space(10)]
    [SerializeField] private bool isCombo = false;
    [SerializeField] private bool forceCombo = false;
    [SerializeField] private int comboAttackCount = 1;
    [SerializeField] private float fullComboCooldown = 0.5f;
    [SerializeField] private float comboResetAfter = 1;

    [Space(10)]
    [SerializeField] private List<StatusEffectData> applyStatusEffects = new List<StatusEffectData>();
    [SerializeField] private List<StatusEffectData> recieveStatusEffects = new List<StatusEffectData>();

    [Space(15)]
    [Header("Attack Stages")]
    [SerializeField] private List<AttackStateMovement> attackStateMovements = new List<AttackStateMovement>();
    
    [Space(5)]
    [Header("Aiming")]
    [SerializeField] private bool hasAimingStage = true;
    [SerializeField] private float maxAimTime = 99;
    [SerializeField] private float attackRequiredChargeUpTime;
    [SerializeField] private float attackCancelCooldown;
    
    [Space(5)]
    [Header("Charging Up")]
    [SerializeField] private float chargingUpTime = 0;

    [Space(5)]
    [Header("Attacking")]
    [SerializeField] private bool hasAttackStage = true;
    [SerializeField] private float maxAttackTime = 99;

    [Space(5)]
    [Header("Cooling Down")]
    [SerializeField] private float coolingDownTime = 0;

    [Space(5)]
    [Header("Cooldown")]
    [SerializeField] private float attackCooldown;

    private float timeAimingStateStarted = int.MinValue;
    private float timeAttackingStateEnded = int.MinValue;

    private bool isClone = false;

    protected IAttack attacker;
    protected IAnimate animator;
    protected Transform parentTransform;
    protected MovementController parentMovementController;

    protected AttackState attackState = AttackState.Idle;

    private bool onCooldown = false;
    protected float timeSinceStateStarted = 0;
    private bool useMovementState = false;
    private StateMovement activeStateMovement;
    private float stateTimeLength = 0;

    private int comboAttack = 0;

    private bool activated = false;

    public string AttackId { get { return attackId; } }
    public float AttackRange { get { return recommendedAttackRange; } }
    public float AttackRangeDeviation { get { return attackRangeDeviation; } }
    public Transform ParentTransform { get { return parentTransform; } }

    public virtual Attack Clone(Attack clone, IAttack attacker, Transform parentTransform)
    {
        if (clone == null)
        {
            clone = CreateInstance(GetType()) as Attack;
        }

        clone.attackId = attackId;
        clone.recommendedAttackRange = recommendedAttackRange;
        clone.attackRangeDeviation = attackRangeDeviation;

        clone.isCombo = isCombo;
        clone.forceCombo = forceCombo;
        clone.comboAttackCount = comboAttackCount;
        clone.fullComboCooldown = fullComboCooldown;
        clone.comboResetAfter = comboResetAfter;

        clone.applyStatusEffects = applyStatusEffects;
        clone.recieveStatusEffects = recieveStatusEffects;
        clone.attackStateMovements = attackStateMovements;

        clone.hasAimingStage = hasAimingStage;
        clone.maxAimTime = maxAimTime;
        clone.attackRequiredChargeUpTime = attackRequiredChargeUpTime;
        clone.attackCancelCooldown = attackCancelCooldown;
        clone.chargingUpTime = chargingUpTime;
        clone.hasAttackStage = hasAttackStage;
        clone.maxAttackTime = maxAttackTime;
        clone.coolingDownTime = coolingDownTime;
        clone.attackCooldown = attackCooldown;

        clone.attacker = attacker;
        clone.parentTransform = parentTransform;
        clone.parentTransform.TryGetComponent(out clone.animator);
        clone.parentTransform.TryGetComponent(out clone.parentMovementController);

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
            if (!hasAimingStage && attackState == AttackState.Aiming)
            {
                attacker.InitiateAttackState(AttackState.ChargingUp);
                return;
            } 
            else if (!hasAttackStage && attackState == AttackState.Attacking)
            {
                attackState = AttackState.CoolingDown;
            }

            OnAttackStateEnd(this.attackState);
            this.attackState = attackState;
            InitiateAttackState(attackState);
        } 
        else
        {
            if (this.attackState == AttackState.ChargingUp && attackState == AttackState.Attacking)
            {
                OnAttackStateCancel(this.attackState, false);
            } 
            else if (this.attackState == AttackState.Aiming && attackState == AttackState.ChargingUp)
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
            activated = false;
        }
        else if (!activated)
        {
            activated = true;
            animator?.TriggerAnimation("Attack1");
        }
    }

    public virtual bool CanInitiateAttackState(AttackState attackState)
    {
        if (!isClone)
        {
            return false;
        }

        if (attackState == AttackState.ChargingUp)
        {
            return (Time.timeSinceLevelLoad - timeAimingStateStarted >= attackRequiredChargeUpTime);
        }
        else if (attackState == AttackState.Aiming)
        {
            return !onCooldown;
        }

        return true;
    }

    public virtual bool IsOnCooldown()
    {
        return onCooldown;
    }

    public virtual void OnAttackStateStart(AttackState attackState)
    {
        attacker.OnAttackStateStart(attackState, AttackId);

        if (attackState == AttackState.Aiming)
        {
            PlayAnimation(AnimationEvent.Aiming, AttackId);
            timeAimingStateStarted = Time.timeSinceLevelLoad;
        } 
        else if (attackState == AttackState.ChargingUp)
        {
            PlayAnimation(AnimationEvent.Charging, AttackId);
            onCooldown = true;
            if (isCombo)
            {
                comboAttack++;
            }
        }
        else if (attackState == AttackState.Attacking)
        {
            PlayAnimation(AnimationEvent.Activating, AttackId);
            if (recieveStatusEffects.Count > 0 && parentTransform.TryGetComponent(out IStatusEffectTarget statusEffectTarget))
            {
                statusEffectTarget.StatusEffectHolder.AddStatusEffect(recieveStatusEffects);
            }
        }
        else if (attackState == AttackState.CoolingDown)
        {
            PlayAnimation(AnimationEvent.Cooling, AttackId);
        }

        timeSinceStateStarted = 0;
        useMovementState = false;

        if (parentMovementController != null)
        {
            foreach (AttackStateMovement attackStateMovement in attackStateMovements)
            {
                if (attackStateMovement.State == attackState)
                {
                    useMovementState = true;
                    activeStateMovement = attackStateMovement.Movement;

                    switch (attackState)
                    {
                        case AttackState.Idle:
                            stateTimeLength = 0;
                            break;
                        case AttackState.Aiming:
                            stateTimeLength = maxAimTime;
                            break;
                        case AttackState.ChargingUp:
                            stateTimeLength = chargingUpTime;
                            break;
                        case AttackState.Attacking:
                            stateTimeLength = maxAttackTime;
                            break;
                        case AttackState.CoolingDown:
                            stateTimeLength = coolingDownTime;
                            break;
                        default:
                            break;
                    }

                    break;
                }
            }
        }
    }

    public virtual void OnAttackState()
    {
        attacker.OnAttackState(attackState);

        timeSinceStateStarted += Time.fixedDeltaTime;

        switch (attackState)
        {
            case AttackState.Idle:
                if (isCombo && timeSinceStateStarted >= comboResetAfter)
                {
                    comboAttack = 0;
                }

                onCooldown = Time.timeSinceLevelLoad - timeAttackingStateEnded <= attackCooldown;
                return;
            case AttackState.Aiming:
                if (maxAimTime < timeSinceStateStarted)
                {
                    TransferToAttackState(AttackState.ChargingUp);
                }
                break;
            case AttackState.ChargingUp:
                if (chargingUpTime < timeSinceStateStarted)
                {
                    TransferToAttackState(AttackState.Attacking);
                }
                break;
            case AttackState.Attacking:
                if (maxAttackTime < timeSinceStateStarted)
                {
                    if (isCombo && forceCombo && comboAttack <= comboAttackCount)
                    {
                        comboAttack++;
                        TransferToAttackState(AttackState.ChargingUp);
                    }
                    else
                    {
                        TransferToAttackState(AttackState.CoolingDown);
                    }
                }
                break;
            case AttackState.CoolingDown:
                if (coolingDownTime < timeSinceStateStarted)
                {
                    TransferToAttackState(AttackState.Idle);
                }
                break;
        }

        if (useMovementState)
        {
            parentMovementController.SetVelocity(activeStateMovement.GetStateCurrentVelocity(timeSinceStateStarted, stateTimeLength, parentTransform, parentMovementController.GetVelocity()));
        }
    }

    public virtual void OnAttackStateEnd(AttackState attackState)
    {
        attacker.OnAttackStateEnd(attackState);

        if (attackState == AttackState.Attacking)
        {
            timeAttackingStateEnded = Time.timeSinceLevelLoad;
            
            if (isCombo)
            {
                if (comboAttack >= comboAttackCount)
                {
                    timeAttackingStateEnded += fullComboCooldown;
                }
            }
        }
    }

    public virtual void OnAttackStateCancel(AttackState attackState, bool otherHasCancelled)
    {
        if (!otherHasCancelled)
        {
            attacker.OnAttackStateCancel(attackState, true);
        }

        this.attackState = AttackState.Idle;

        if (attackState == AttackState.Aiming)
        {
            PlayAnimation(AnimationEvent.Canceling, AttackId);
        }
        else if (attackState == AttackState.Attacking)
        {
            timeAttackingStateEnded = Time.timeSinceLevelLoad - attackCooldown + attackCancelCooldown;
        }
        activated = false;
    }

    public virtual void OnAttackTriggerEnter(Transform hit) { }

    public virtual void OnAttackTriggerStay(Transform hit) { }

    public virtual void OnAttackTriggerExit(Transform hit) { }

    public virtual bool CanHitEntity(Transform hit)
    {
        if (hit.TryGetComponent(out IHealth entityHealth))
        {
            if (attacker.DamageableEntities.Contains(entityHealth.EntityType))
            {
                return true;
            }
        }

        return false;
    }

    public virtual void OnAttackHit(Transform hit, float damage, float knockbackPower)
    {
        if (CanHitEntity(hit))
        {
            hit.GetComponent<IHealth>().Damage(damage);

            if (applyStatusEffects.Count > 0 && hit.TryGetComponent(out IStatusEffectTarget statusEffectTarget))
            {
                statusEffectTarget.StatusEffectHolder.AddStatusEffect(applyStatusEffects);
            }

            Vector3 knockbackDirection = (hit.position - parentTransform.position).normalized;
            ApplyKnockback(hit, knockbackPower, knockbackDirection);
        }
    }

    public virtual void OnProjectileHit(BasicProjectile projectile, Transform hit, float damage, float knockbackPower)
    {
        if (CanHitEntity(hit))
        {
            projectile.DestroyProjectile();

            hit.GetComponent<IHealth>().Damage(damage);

            if (applyStatusEffects.Count > 0 && hit.TryGetComponent(out IStatusEffectTarget statusEffectTarget))
            {
                statusEffectTarget.StatusEffectHolder.AddStatusEffect(applyStatusEffects);
            }

            Vector3 knockbackDirection = projectile.transform.forward;
            ApplyKnockback(hit, knockbackPower, knockbackDirection);
        }
    }

    public virtual void ApplyKnockback(Transform hit, float knockbackPower, Vector3 knockbackDirection)
    {
        if (hit.TryGetComponent(out MovementController parentMovementController))
        {
            parentMovementController.ApplyForce(knockbackDirection.normalized * knockbackPower);
        }
        else if (hit.TryGetComponent(out Rigidbody rig))
        {
            rig.AddForce(knockbackDirection.normalized * knockbackPower, ForceMode.Impulse);
        }
    }

    public void PlayAudio(EventReference audioReference)
    {
        if (!audioReference.IsNull)
        {
            AudioManager.Instance.PlayOneShot(audioReference, parentTransform.position);
        }
    }

    public void PlayAnimation(AnimationEvent animationEvent, string animationId)
    {
        animator?.OnAnimationEventStart(animationEvent, animationId);
    }

    public virtual GameObject SpawnProjectile(ProjectileData projectileData, Vector3 position, Vector3 rotation)
    {
        GameObject spawnedProjectile = Instantiate(projectileData.ProjectilePrefab, position, Quaternion.Euler(rotation));
        BasicProjectile basicProjectile = spawnedProjectile.GetComponent<BasicProjectile>();

        if (basicProjectile == null)
        {
            Debug.LogWarning($"{spawnedProjectile.name} is missing a basic projectile script");
        } 
        else
        {
            basicProjectile.SetProjectileData(projectileData);
            basicProjectile.SetParentAttack(this);
        }

        return spawnedProjectile;
    }

    public virtual void FireProjectile(ProjectileData projectileData, GameObject projectile, Vector3 direction)
    {
        Rigidbody rig = projectile.GetComponent<Rigidbody>();

        rig.AddForce(projectileData.FireSpeed * direction.normalized, ForceMode.Impulse);

        projectile.GetComponent<BasicProjectile>().OnFireProjectile();
    }
}

[Serializable]
public class AttackStateMovement
{
    [SerializeField] private AttackState state;
    [SerializeField] private StateMovement movement;

    public AttackState State { get { return state; } }
    public StateMovement Movement { get { return movement; } }
}