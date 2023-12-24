using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour, IHealth, IEnemy, IAttack, IDodge, IPathfind, IStatusEffectTarget
{
    // ---------------------------------------------------------------------------------------------------------
    // Enemy Variables
    // ---------------------------------------------------------------------------------------------------------

    [Header("Enemy")]
    [SerializeField] private float chaseActivationDistance = 10;

    [SerializeField] private float timeBetweenWanders = 5;
    [Range(5, 100)]
    [SerializeField] private float chanceToWander = 50;
    [SerializeField] private float wanderRange = 5;
    [SerializeField] private float attackRange = 4;
    [SerializeField] private float stoppingRange = 3;
    [SerializeField] private float maxTotalWanderRange = 10;
    [SerializeField] private float lostPlayerRecoverTime = 4;

    private PlayerController player;
    private RigidAgent rigidAgent;

    private float timeSinceLastWander = 0;
    private float timeSinceLostPlayer = 0;

    private Vector3 spawnPosition;

    // ---------------------------------------------------------------------------------------------------------
    // Interface Related Variables
    // ---------------------------------------------------------------------------------------------------------

    [Header("Health")]
    [SerializeField] private float maxHealth;
    private readonly EntityType entityType = EntityType.Enemy;
    private float health;
    private bool isInvincible;

    private EnemyState enemyState;

    [Space(10)]
    [Header("Attacking")]
    [SerializeField] private List<EntityType> damageableEntities;
    private AttackHolder attackHolder;
    private AttackState attackState;
    private bool isAttacking;

    [Space(10)]
    [Header("Dodging")]
    private DodgeHolder dodgeHolder;
    private DodgeState dodgeState;
    private bool isDodging;

    private bool isPathfinding;
    private Vector3 pathfindDestination;
    private NavMeshAgent navMeshAgent;

    private StatusEffectHolder statusEffectHolder;

    // ---------------------------------------------------------------------------------------------------------
    // Interface Implementation Fields
    // ---------------------------------------------------------------------------------------------------------

    public EntityType EntityType { get { return entityType; } }
    public float MaxHealth { get { return maxHealth; } }
    public float Health { get { return health; } }
    public bool IsInvincible { get { return isInvincible; } }
    public EnemyState EnemyState { get { return enemyState; } }
    public AttackHolder AttackHolder { get { return attackHolder; } }
    public bool IsAttacking { get { return isAttacking; } }
    public AttackState AttackState { get { return attackState; } }
    public List<EntityType> DamageableEntities { get { return damageableEntities; } }
    public DodgeHolder DodgeHolder { get { return dodgeHolder; } }
    public bool IsDodging { get { return isDodging; } }
    public DodgeState DodgeState { get { return dodgeState; } }
    public bool IsPathfinding { get { return isPathfinding; } }
    public Vector3 PathfindDestination { get { return pathfindDestination; } set { pathfindDestination = value; } }
    public StatusEffectHolder StatusEffectHolder { get { return statusEffectHolder; } }

    // ---------------------------------------------------------------------------------------------------------
    // Default Unity Methods
    // ---------------------------------------------------------------------------------------------------------

    protected virtual void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        rigidAgent = GetComponent<RigidAgent>();

        if (!TryGetComponent(out attackHolder))
        {
            attackHolder = transform.AddComponent<AttackHolder>();
        }

        if (!TryGetComponent(out dodgeHolder))
        {
            dodgeHolder = transform.AddComponent<DodgeHolder>();
        }

        if (!TryGetComponent(out statusEffectHolder))
        {
            statusEffectHolder = transform.AddComponent<StatusEffectHolder>();
        }

        health = maxHealth;
    }

    protected virtual void Start()
    {
        player = PlayerController.Instance;

        spawnPosition = transform.position;

        InitiateEnemyState(EnemyState.Idle);
    }

    private void FixedUpdate()
    {
        OnEnemyState(enemyState);
    }

    // ---------------------------------------------------------------------------------------------------------
    // Interface Implementation Methods
    // ---------------------------------------------------------------------------------------------------------

    public virtual void Damage(float damage)
    {
        if (isInvincible)
        {
            return;
        }

        health = Mathf.Max(health - damage, 0);

        EffectManager.Instance.Flash(transform);

        if (health == 0)
        {
            Die();
        }
    }

    public virtual void Heal(float healing)
    {
        health = Mathf.Min(health + healing, maxHealth);
    }

    public virtual void Die()
    {
        if (isAttacking)
        {
            OnAttackStateCancel(attackState, false);
        }
        if (isDodging)
        {
            OnDodgeStateCancel(dodgeState, false);
        }

        statusEffectHolder.ClearStatusEffects();
        attackHolder.DestroyClones();
        dodgeHolder.DestroyClones();

        CancelPathfinding();

        Destroy(gameObject);
    }

    public virtual void TransferToEnemyState(EnemyState enemyState)
    {
        OnEnemyStateEnd(this.enemyState);

        this.enemyState = enemyState;

        OnEnemyStateStart(enemyState);
    }

    public virtual void InitiateEnemyState(EnemyState enemyState)
    {
        if (CanInitiateEnemyState(enemyState))
        {
            TransferToEnemyState(enemyState);
        }
    }

    public virtual bool CanInitiateEnemyState(EnemyState enemyState)
    {
        return true;
    }

    public virtual void OnEnemyStateStart(EnemyState enemyState)
    {
        switch (enemyState)
        {
            case EnemyState.Deciding:

                break;
            case EnemyState.Idle:

                break;
            case EnemyState.Patrolling:
                pathfindDestination = spawnPosition;
                InitiatePathfinding();
                break;
            case EnemyState.Chasing:

                break;
            case EnemyState.Repositioning:

                break;
            case EnemyState.Attacking:

                break;
            case EnemyState.Dodging: 
                
                break;
            case EnemyState.Fleeing:

                break;
            case EnemyState.Stunned:

                break;
            case EnemyState.Dead:

                break;
        }
    }

    public virtual void OnEnemyState(EnemyState enemyState)
    {
        switch (enemyState)
        {
            case EnemyState.Deciding:
                OnDeciding();
                break;
            case EnemyState.Idle:
                OnIdle();
                break;
            case EnemyState.Patrolling:
                OnPatrolling();
                break;
            case EnemyState.Chasing:
                OnChasing();
                break;
            case EnemyState.Repositioning:
                OnRepositioning();
                break;
            case EnemyState.Attacking:
                OnAttacking();
                break;
            case EnemyState.Dodging:
                OnDodging();
                break;
            case EnemyState.Fleeing:
                OnFleeing();
                break;
            case EnemyState.Stunned:
                OnStunned();
                break;
            case EnemyState.Dead:
                OnDead();
                break;
        }
    }

    public virtual void OnEnemyStateEnd(EnemyState enemyState)
    {
        switch (enemyState)
        {
            case EnemyState.Deciding:

                break;
            case EnemyState.Idle:

                break;
            case EnemyState.Patrolling:

                break;
            case EnemyState.Chasing:

                break;
            case EnemyState.Repositioning:

                break;
            case EnemyState.Attacking:

                break;
            case EnemyState.Dodging:
                
                break;
            case EnemyState.Fleeing:

                break;
            case EnemyState.Stunned:

                break;
            case EnemyState.Dead:

                break;
        }
    }

    public virtual void TransferToAttackState(AttackState attackState)
    {
        this.attackState = attackState;
    }

    public virtual void InitiateAttackState(AttackState attackState)
    {
        if (CanInitiateAttackState(attackState, attackHolder.GetAttackId()))
        {
            attackHolder.GetActiveAttack().TransferToAttackState(attackState);
        }
    }

    public virtual bool CanInitiateAttackState(AttackState attackState, string attackId)
    {
        if (!attackHolder.CanAttack())
        {
            return false;
        }
        else if (attackState == AttackState.Aiming && (isAttacking || isDodging || isPathfinding))
        {
            return false;
        } 
        else if (this.attackState == attackState)
        {
            return false;
        }

        return true;
    }

    public virtual void OnAttackStateStart(AttackState attackState)
    {
        // Actions to do when the state has first started
        if (attackState == AttackState.Idle)
        {
            rigidAgent.SetAllowMovementInput(true);
            rigidAgent.SetAllowRotationInput(true);

            isAttacking = false;
            return;
        }
        else
        {
            isAttacking = true;
        }

        if (attackState == AttackState.Aiming)
        {
            rigidAgent.SetAllowMovementInput(false);
        } 
        else if (attackState == AttackState.ChargingUp)
        {
            rigidAgent.SetAllowMovementInput(false);
            rigidAgent.SetAllowRotationInput(false);
        }
        else if (attackState == AttackState.Attacking)
        {
            rigidAgent.SetAllowMovementInput(false);
            rigidAgent.SetAllowRotationInput(false);
        }
        else if (attackState == AttackState.CoolingDown)
        {
            rigidAgent.SetAllowMovementInput(false);
            rigidAgent.SetAllowRotationInput(false);
        }
    }

    public virtual void OnAttackState(AttackState attackState)
    {
        // Actions to do while the state is happening
        if (attackState == AttackState.Aiming)
        {
            Aim();
        }
    }

    public virtual void OnAttackStateEnd(AttackState attackState)
    {
        // Actions to do when the state has ended
        rigidAgent.SetAllowMovementInput(true);
        rigidAgent.SetAllowRotationInput(true);
    }

    public virtual void OnAttackStateCancel(AttackState attackState, bool otherHasCancelled)
    {
        // Actions to do when the state is cancelled
        if (!otherHasCancelled)
        {
            if (attackState == AttackState.Idle)
            {
                // Cancel called while idling
                return;
            }

            attackHolder.GetActiveAttack().OnAttackStateCancel(attackState, true);
        }

        this.attackState = AttackState.Idle;
        rigidAgent.SetAllowMovementInput(true);
        rigidAgent.SetAllowRotationInput(true);
        isAttacking = false;
    }

    public void TransferToDodgeState(DodgeState dodgeState)
    {
        this.dodgeState = dodgeState;
    }

    public void InitiateDodgeState(DodgeState dodgeState)
    {
        if (CanInitiateDodgeState(dodgeState, dodgeHolder.GetDodgeId()))
        {
            dodgeHolder.GetActiveDodge().TransferToDodgeState(dodgeState);
        }
    }

    public bool CanInitiateDodgeState(DodgeState dodgeState, string dodgeId)
    {
        if (!dodgeHolder.CanDodge())
        {
            return false;
        }
        else if (dodgeState == DodgeState.Aiming && (isAttacking || IsDodging || isPathfinding))
        {
            return false;
        }
        else if (this.dodgeState == dodgeState)
        {
            return false;
        }

        return true;
    }

    public void OnDodgeStateStart(DodgeState dodgeState)
    {
        // Actions to do when the state has first started
        if (dodgeState == DodgeState.Idle)
        {
            rigidAgent.SetAllowMovementInput(true);
            rigidAgent.SetAllowRotationInput(true);

            isDodging = false;
            return;
        }
        else
        {
            isDodging = true;
        }

        if (dodgeState == DodgeState.Aiming)
        {
            rigidAgent.SetAllowMovementInput(false);
        }
        else if (dodgeState == DodgeState.ChargingUp)
        {
            rigidAgent.SetAllowMovementInput(false);
            rigidAgent.SetAllowRotationInput(false);
        }
        else if (dodgeState == DodgeState.Dodging)
        {
            rigidAgent.SetAllowMovementInput(false);
            rigidAgent.SetAllowRotationInput(false);
        }
        else if (dodgeState == DodgeState.CoolingDown)
        {
            rigidAgent.SetAllowMovementInput(false);
            rigidAgent.SetAllowRotationInput(false);
        }
    }

    public void OnDodgeState(DodgeState dodgeState)
    {
        // Actions to do while the state is happening
        if (dodgeState == DodgeState.Aiming)
        {
            Aim();
        }
    }

    public void OnDodgeStateEnd(DodgeState dodgeState)
    {
        // Actions to do when the state has ended
        rigidAgent.SetAllowMovementInput(true);
        rigidAgent.SetAllowRotationInput(true);
    }

    public void OnDodgeStateCancel(DodgeState dodgeState, bool otherHasCancelled)
    {
        // Actions to do when the state is cancelled
        if (!otherHasCancelled)
        {
            if (dodgeState == DodgeState.Idle)
            {
                // Cancel called while idling
                return;
            }

            dodgeHolder.GetActiveDodge().OnDodgeStateCancel(dodgeState, true);
        }

        this.dodgeState = DodgeState.Idle;
        rigidAgent.SetAllowMovementInput(true);
        rigidAgent.SetAllowRotationInput(true);
        isDodging = false;
    }

    public void TransferToPathfindingState(bool isPathfinding)
    {
        this.isPathfinding = isPathfinding;
    }

    public virtual void InitiatePathfinding()
    {
        if (CanInitiatePathfinding())
        {
            rigidAgent.InitiatePathfinding(transform, pathfindDestination);
        }
    }

    public virtual bool CanInitiatePathfinding()
    {
        return (!isDodging && attackState == AttackState.Idle);
    }

    public virtual void OnPathfinding()
    {

    }

    public virtual void CancelPathfinding()
    {
        rigidAgent.StopPathfinding();
    }

    // ---------------------------------------------------------------------------------------------------------
    // Extra Methods
    // ---------------------------------------------------------------------------------------------------------

    public virtual void OnDeciding()
    {

    }

    public virtual void OnIdle()
    {
        if (IsWithinChasingDistance())
        {
            InitiateEnemyState(EnemyState.Chasing);
            return;
        }

        if ((spawnPosition - transform.position).magnitude > maxTotalWanderRange)
        {
            if (Time.timeSinceLevelLoad - timeSinceLostPlayer >= lostPlayerRecoverTime)
            {
                InitiateEnemyState(EnemyState.Patrolling);
                return;
            }
        }

        if (Time.timeSinceLevelLoad - timeSinceLastWander >= timeBetweenWanders)
        {
            InitiateWander();
            timeSinceLastWander = Time.timeSinceLevelLoad;
        }
    }

    public virtual void OnPatrolling()
    {
        if (IsWithinChasingDistance())
        {
            InitiateEnemyState(EnemyState.Chasing);
            return;
        }

        if (isPathfinding)
        {
            if ((pathfindDestination - transform.position).magnitude <= Mathf.Min(wanderRange / 2f, maxTotalWanderRange / 2f))
            {
                CancelPathfinding();
                InitiateEnemyState(EnemyState.Idle);
                return;
            }
        }
    }

    public virtual void OnChasing()
    {
        Vector3 vectorToPlayer = GetVectorToPlayer();

        pathfindDestination = player.transform.position - vectorToPlayer.normalized * stoppingRange;

        // If stopping distance is greater than 0, player will no longer be pathfinding when this needs to trigger

        if (isPathfinding)
        {
            if (vectorToPlayer.magnitude < attackRange)
            {
                CancelPathfinding();
                InitiateEnemyState(EnemyState.Attacking);
            }
        }
        else
        {
            InitiatePathfinding();
        }

        if (!IsWithinChasingDistance())
        {
            timeSinceLostPlayer = Time.timeSinceLevelLoad;
            InitiateEnemyState(EnemyState.Idle);
        }
    }

    public virtual void OnRepositioning()
    {

    }

    public virtual void OnAttacking()
    {
        if (GetVectorToPlayer().magnitude > attackRange && !isAttacking)
        {
            InitiateEnemyState(EnemyState.Chasing);
            return;
        }

        if (!isAttacking)
        {
            Aim();
            InitiateAttackState(AttackState.Aiming);
        }
    }

    public virtual void OnDodging()
    {

    }

    public virtual void OnFleeing()
    {

    }

    public virtual void OnStunned()
    {

    }

    public virtual void OnDead()
    {

    }

    public virtual void Aim()
    {
        Vector3 targetDirection = GetVectorToPlayer();
        targetDirection.y = 0;

        rigidAgent.SetRotation(Quaternion.Inverse(MovementController.MovementAxis) * targetDirection);
    }

    public virtual void InitiateWander()
    {
        if (!RollDice(chanceToWander))
        {
            return;
        }

        for (int wanderTry = 1; wanderTry <= 5; wanderTry++)
        {
            Vector3 wanderDestination = transform.position + new Vector3(Random.Range(-wanderRange, wanderRange), 0, Random.Range(-wanderRange, wanderRange));

            if (NavMesh.SamplePosition(wanderDestination, out _, navMeshAgent.height * 2, NavMesh.AllAreas))
            {
                pathfindDestination = wanderDestination;

                InitiatePathfinding();

                return;
            }
        }
    }

    public bool IsWithinChasingDistance()
    {
        return (GetVectorToPlayer().magnitude <= chaseActivationDistance);
    }

    public Vector3 GetVectorToPlayer()
    {
        return player.transform.position - transform.position;
    }

    public bool RollDice(float chance)
    {
        return chance <= Random.Range(0, 100);
    }
}