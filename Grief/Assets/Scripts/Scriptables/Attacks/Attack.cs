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

    [Space(10)]
    [SerializeField] private List<AttackEffectType> attackEffects = new List<AttackEffectType>();

    [Space(10)]
    [SerializeField] private float attackCooldown;
    [SerializeField] private float attackCancelCooldown;
    [SerializeField] private float attackRequiredChargeUpTime;

    [Space(10)]
    [SerializeField] private bool hasAimingStage = true;
    [SerializeField] private bool hasAttackStage = true;

    [Space(10)]
    [SerializeField] private float maxAimTime = 99;
    [SerializeField] private float maxAttackTime = 99;

    [Space(10)]
    [Header("Specifically for Charging Up Stage")]
    [SerializeField] private float chargingUpTime = 0;
    [Header("Specifically for Cooling Down Stage")]
    [SerializeField] private float coolingDownTime = 0;

    private float timeAimingStateStarted = int.MinValue;
    private float timeAttackingStateEnded = int.MinValue;

    private bool isClone = false;

    protected IAttack attacker;
    protected Transform parentTransform;
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

        clone.attackEffects = attackEffects;
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

        while (true)
        {
            yield return new WaitForFixedUpdate();
            OnAttackState(attackState, Time.timeSinceLevelLoad - timeStateStarted);
        }
    }

    // Currently Unfinished
    public virtual bool OnAttackTriggerEnter(IHealth entity, Transform entityTransform)
    {
        return CombatManager.Instance.DamageEntity(this, attacker, entity, parentTransform, entityTransform);
        //CombatManager.Instance.ApplyKnockback(this, parentTransform, entityTransform);
    }

    public virtual void OnAttackTriggerStay(IHealth entity, Transform entityTransform)
    {

    }

    public virtual void OnAttackTriggerExit(IHealth entity, Transform entityTransform)
    {

    }

    public void PlayAudio(string audioId)
    {
        if (audioId != "")
        {
            AudioManager.Instance.PlaySound(audioId);
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

public class AttackEffect
{

}