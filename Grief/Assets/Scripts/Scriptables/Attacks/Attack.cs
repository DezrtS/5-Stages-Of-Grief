using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Attack : ScriptableObject
{
    [Header("Attack Variables")]
    [SerializeField] private string attackId;

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
    protected Transform parentTransform;

    // Put in proper place
    protected MovementController movementController;
    private IEnumerator attackStateCoroutine;

    private AttackState attackState = AttackState.Idle;

    public string AttackId { get { return attackId; } }
    public Transform ParentTransform { get { return parentTransform; } }
    public AttackState AttackState { get { return attackState; } }

    public virtual Attack Clone(Attack clone, IAttack attacker, Transform parentTransform)
    {
        if (clone == null)
        {
            clone = CreateInstance(GetType()) as Attack;
        }

        clone.attackId = attackId;

        clone.applyStatusEffects = applyStatusEffects;
        clone.recieveStatusEffects = recieveStatusEffects;
        clone.attackCooldown = attackCooldown;
        clone.attackCancelCooldown = attackCancelCooldown;
        clone.attackRequiredChargeUpTime = attackRequiredChargeUpTime;
        clone.hasAimingStage = hasAimingStage;
        clone.hasAttackStage = hasAttackStage;
        clone.maxAimTime = maxAimTime;
        clone.maxAttackTime = maxAttackTime;
        clone.chargingUpTime = chargingUpTime;
        clone.coolingDownTime = coolingDownTime;

        clone.attacker = attacker;
        clone.parentTransform = parentTransform;

        clone.isClone = true;

        // Reorganize in proper places
        clone.attackStateMovements = attackStateMovements;
        clone.parentTransform.TryGetComponent(out clone.movementController);

        return clone;
    }

    public virtual void DestroyClone()
    {
        Destroy(this);
    }

    public abstract float GetDamage();

    public virtual void TransferToAttackState(AttackState attackState)
    {
        if (CanInitiateAttackState(attackState))
        {
            OnAttackStateEnd(this.attackState);

            if (!hasAimingStage && attackState == AttackState.Aiming)
            {
                attacker.InitiateAttackState(AttackState.ChargingUp);
                return;
            } 
            else if (!hasAttackStage && attackState == AttackState.Attacking)
            {
                attackState = AttackState.CoolingDown;
            }

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

        if (attackState == AttackState.ChargingUp)
        {
            return (Time.timeSinceLevelLoad - timeAimingStateStarted >= attackRequiredChargeUpTime);
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
        else if (attackState == AttackState.Attacking)
        {
            if (recieveStatusEffects.Count > 0 && parentTransform.TryGetComponent(out IStatusEffectTarget statusEffectTarget))
            {
                statusEffectTarget.StatusEffectHolder.AddStatusEffect(recieveStatusEffects);
            }
        }
    }

    public virtual void OnAttackState(AttackState attackState, float timeSinceStateStarted)
    {
        attacker.OnAttackState(attackState);

        switch (attackState)
        {
            case AttackState.Idle:
                Debug.LogWarning("Idle On Attack State");
                break;
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
                    TransferToAttackState(AttackState.CoolingDown);
                }
                break;
            case AttackState.CoolingDown:
                if (coolingDownTime < timeSinceStateStarted)
                {
                    TransferToAttackState(AttackState.Idle);
                }
                break;
        }
    }

    public virtual void OnAttackStateEnd(AttackState attackState)
    {
        attacker.OnAttackStateEnd(attackState);

        CoroutineRunner.Instance.StopCoroutine(attackStateCoroutine);

        if (attackState == AttackState.Attacking)
        {
            timeAttackingStateEnded = Time.timeSinceLevelLoad;
        }
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

        bool useMovementState = false;
        StateMovement stateMovement = null;
        float stateTimeLength = 0;

        if (movementController != null)
        {
            foreach (AttackStateMovement attackStateMovement in attackStateMovements)
            {
                if (attackStateMovement.State == attackState)
                {
                    useMovementState = true;
                    stateMovement = attackStateMovement.Movement;

                    switch (attackState)
                    {
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

        while (true)
        {
            yield return new WaitForFixedUpdate();

            if (useMovementState)
            {
                movementController.SetVelocity(stateMovement.GetStateCurrentVelocity(Time.timeSinceLevelLoad - timeStateStarted, stateTimeLength, parentTransform, movementController.GetVelocity()));
            }

            OnAttackState(attackState, Time.timeSinceLevelLoad - timeStateStarted);
        }
    }

    // All Code Below Needs Tweaking
    public virtual bool OnAttackTriggerEnter(IHealth entity, Transform entityTransform)
    {
        // Right now status effects do not care if they can actually be applied to the entity type
        if (applyStatusEffects.Count > 0 && entityTransform.TryGetComponent(out IStatusEffectTarget statusEffectTarget))
        {
            statusEffectTarget.StatusEffectHolder.AddStatusEffect(applyStatusEffects);
        }

        return CombatManager.Instance.DamageEntity(this, attacker, entity, parentTransform, entityTransform);
        //CombatManager.Instance.ApplyKnockback(this, parentTransform, entityTransform);
    }

    public virtual void OnAttackTriggerStay(IHealth entity, Transform entityTransform)
    {

    }

    public virtual void OnAttackTriggerExit(IHealth entity, Transform entityTransform)
    {

    }

    public void PlayAudio(EventReference audioReference)
    {
        if (!audioReference.IsNull)
        {
            AudioManager.Instance.PlayOneShot(audioReference, parentTransform.position);
        }
    }

    public void DamageEntity(IHealth entity)
    {
        CombatManager.Instance.DamageEntity(this, attacker, entity);
    }

    public virtual GameObject SpawnProjectile(Projectile projectileData, Vector3 position, Vector3 rotation)
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

    public virtual void FireProjectile(Projectile projectileData, GameObject projectile, Vector3 direction)
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