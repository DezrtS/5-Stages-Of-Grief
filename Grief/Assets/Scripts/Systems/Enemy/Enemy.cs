using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IHealth, IEnemy, IAttack, IDodge, IPathfind, IStatusEffectTarget, IAnimate
{
    // ---------------------------------------------------------------------------------------------------------
    // Enemy Variables
    // ---------------------------------------------------------------------------------------------------------

    [Header("Enemy")]
    [SerializeField] private float chaseActivationDistance = 10;
    [SerializeField] private float defaultRepositioningDistance = 5;
    [SerializeField] private float defaultRepositioningDeviationDistance = 5;

    [SerializeField] private float timeBetweenWanders = 5;
    [Range(5, 100)]
    [SerializeField] private float chanceToWander = 50;
    [SerializeField] private float wanderRange = 5;
    [SerializeField] private float maxTotalWanderRange = 10;
    [SerializeField] private float lostPlayerRecoverTime = 4;

    private float attackRange = 1;
    private float attackRangeDeviation = 1;

    private bool chooseNewAttack;

    private PlayerController player;
    private RigidAgent rigidAgent;

    private float timeSinceLastWander = 0;
    private float timeSinceLostPlayer = 0;

    private Vector3 spawnPosition;

    private bool isOnCooldown;

    private float timer = 0;
    private float spawnTime = 2;
    private float dyingTime = 2;

    // ---------------------------------------------------------------------------------------------------------
    // Interface Related Variables
    // ---------------------------------------------------------------------------------------------------------

    [Header("Health")]
    [SerializeField] private float maxHealth;
    private readonly EntityType entityType = EntityType.Enemy;
    private float health;
    private bool isInvincible;

    private EnemyState enemyState;
    private bool isQueued;
    private bool hasRequested;

    [Space(10)]
    [Header("Attacking")]
    [SerializeField] private List<EntityType> damageableEntities;
    private AttackHolder attackHolder;
    private GameObject particleEffectHolder;
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

    [SerializeField] private Animator animator;
    private bool canAnimate = true;

    // ---------------------------------------------------------------------------------------------------------
    // Interface Implementation Fields
    // ---------------------------------------------------------------------------------------------------------

    public EntityType EntityType { get { return entityType; } }
    public float MaxHealth { get { return maxHealth; } }
    public float Health { get { return health; } }
    public bool IsInvincible { get { return isInvincible; } }
    public EnemyState EnemyState { get { return enemyState; } }
    public bool IsQueued { get { return isQueued; } set { isQueued = value; } }
    public bool HasRequested { get { return hasRequested; } set { hasRequested = value; } }
    public AttackHolder AttackHolder { get { return attackHolder; } }
    public GameObject ParticleEffectHolder { get { return particleEffectHolder; } }
    public bool IsAttacking { get { return isAttacking; } }
    public AttackState AttackState { get { return attackState; } }
    public List<EntityType> DamageableEntities { get { return damageableEntities; } }
    public DodgeHolder DodgeHolder { get { return dodgeHolder; } }
    public bool IsDodging { get { return isDodging; } }
    public DodgeState DodgeState { get { return dodgeState; } }
    public bool IsPathfinding { get { return isPathfinding; } }
    public Vector3 PathfindDestination { get { return pathfindDestination; } set { pathfindDestination = value; } }
    public StatusEffectHolder StatusEffectHolder { get { return statusEffectHolder; } }
    public Animator Animator { get { return animator; } }
    public bool CanAnimate { get { return canAnimate; } }

    // ---------------------------------------------------------------------------------------------------------
    // Class Events
    // ---------------------------------------------------------------------------------------------------------

    public delegate void EnemyDeathHandler(Enemy enemy);

    public event EnemyDeathHandler OnEnemyDeath;

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

        if (animator == null)
        {
            canAnimate = false;
        }

        health = maxHealth;
    }

    protected virtual void Start()
    {
        player = PlayerController.Instance;
        EnemyManager.Instance.AddEnemyToList(this);

        particleEffectHolder = Instantiate(GameManager.Instance.EmptyGameObject, transform);
        particleEffectHolder.name = $"{name}'s Particle Effect Holder";

        spawnPosition = transform.position;

        if (attackHolder.CanAttack())
        {
            attackRange = attackHolder.GetActiveAttack().AttackRange;
            attackRangeDeviation = attackHolder.GetActiveAttack().AttackRangeDeviation;
        }

        InitiateEnemyState(EnemyState.Spawning);

        EffectManager.Instance.Dissolve(transform, true);
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
        OnAnimationEventStart(AnimationEvent.Hurt, "");

        EffectManager.Instance.Flash(transform);
        //AudioManager.Instance.PlayOneShot(FMODEventsManager.Instance.hit, transform.position);

        if (health == 0)
        {
            TransferToEnemyState(EnemyState.Dying);
            //Die();
        }
    }

    public virtual void Heal(float healing)
    {
        health = Mathf.Min(health + healing, maxHealth);
    }

    public virtual void Die()
    {
        OnAnimationEventStart(AnimationEvent.Die, "");

        if (isQueued)
        {
            EnemyQueueManager.Instance.CycleOut(this);
        }
        else if (hasRequested)
        {
            EnemyQueueManager.Instance.RemoveFromQueue(this);
        }

        OnEnemyDeath?.Invoke(this);

        statusEffectHolder.ClearStatusEffects();
        attackHolder.DestroyClones();
        dodgeHolder.DestroyClones();

        //CancelPathfinding();

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
            case EnemyState.Spawning:
                timer = 0;
                isInvincible = true;
                break;
            case EnemyState.Idle:

                break;
            case EnemyState.Patrolling:
                pathfindDestination = spawnPosition;
                InitiatePathfinding();
                break;
            case EnemyState.Chasing:
                //AudioManager.Instance.PlayOneShot(FMODEventsManager.Instance.snarl, transform.position);
                break;
            case EnemyState.Repositioning:
                if (navMeshAgent != null)
                {
                    navMeshAgent.updateRotation = false;
                }
                break;
            case EnemyState.Attacking:

                break;
            case EnemyState.Dodging: 
                
                break;
            case EnemyState.Fleeing:

                break;
            case EnemyState.Stunned:

                break;
            case EnemyState.Dying:
                if (isAttacking)
                {
                    OnAttackStateCancel(attackState, false);
                }
                if (isDodging)
                {
                    OnDodgeStateCancel(dodgeState, false);
                }

                EnemyManager.Instance.RemoveEnemyFromList(this);

                EffectManager.Instance.Dissolve(transform, false);
                CancelPathfinding();
                timer = 0;
                isInvincible = true;
                break;
        }
    }

    public virtual void OnEnemyState(EnemyState enemyState)
    { 
        switch (enemyState)
        {
            case EnemyState.Spawning:
                OnSpawning(); 
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
            case EnemyState.Dying:
                OnDying();
                break;
        }
    }

    public virtual void OnEnemyStateEnd(EnemyState enemyState)
    {
        switch (enemyState)
        {
            case EnemyState.Spawning:
                isInvincible = false;
                break;
            case EnemyState.Idle:

                break;
            case EnemyState.Patrolling:

                break;
            case EnemyState.Chasing:

                break;
            case EnemyState.Repositioning:
                if (navMeshAgent != null)
                {
                    navMeshAgent.updateRotation = true;
                }
                break;
            case EnemyState.Attacking:

                break;
            case EnemyState.Dodging:
                
                break;
            case EnemyState.Fleeing:

                break;
            case EnemyState.Stunned:

                break;
            case EnemyState.Dying:
                isInvincible = false;
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

    public virtual void OnAttackStateStart(AttackState attackState, string attackId) 
    {
        // Actions to do when the state has first started
        if (attackState == AttackState.Idle)
        {
            rigidAgent.SetAllowMovementInput(true);
            rigidAgent.SetAllowRotationInput(true);

            isAttacking = false;

            if (isQueued)
            {
                EnemyQueueManager.Instance.CycleOut(this);
            }
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

    public void TriggerAnimation(string animationId)
    {
        if (canAnimate)
        {
            animator.SetTrigger(animationId);
        }
    }

    public void OnAnimationEventStart(AnimationEvent animationEvent, string animationId)
    {
        if (!canAnimate)
        {
            return;
        }

        // Put check to see if animator has parameter.

        switch (animationEvent)
        {
            case AnimationEvent.Aiming:
                animator.SetTrigger("Aiming");
                break;
            case AnimationEvent.Charging:
                animator.SetTrigger("Charging");
                break;
            case AnimationEvent.Activating:
                animator.SetTrigger("Activating");
                break;
            case AnimationEvent.Cooling:
                animator.SetTrigger("Cooling");
                break;
            case AnimationEvent.Canceling:
                animator.SetTrigger("Cancelling");
                break;
            case AnimationEvent.Hurt:
                //animator.SetTrigger("Hurt");
                break;
            case AnimationEvent.Die:
                //animator.SetTrigger("Die");
                break;
            default:
                Debug.Log($"Animation Event {animationEvent} is not supported for {name}");
                break;
        }
    }

    // ---------------------------------------------------------------------------------------------------------
    // Extra Methods
    // ---------------------------------------------------------------------------------------------------------

    public virtual void OnSpawning()
    {
        timer += Time.fixedDeltaTime;

        if (timer >= spawnTime)
        {
            TransferToEnemyState(EnemyState.Idle);
        }
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

        pathfindDestination = player.transform.position - vectorToPlayer.normalized * attackRange;

        if (isPathfinding)
        {
            if (vectorToPlayer.magnitude < attackRange + attackRangeDeviation)
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
        Vector3 vectorToPlayer = GetVectorToPlayer();

        pathfindDestination = player.transform.position - vectorToPlayer.normalized * attackRange;

        Aim();

        if (isPathfinding)
        {
            if (isOnCooldown && !attackHolder.GetActiveAttack().IsOnCooldown())
            {
                attackRange = attackHolder.GetActiveAttack().AttackRange;
                attackRangeDeviation = attackHolder.GetActiveAttack().AttackRangeDeviation;
                isOnCooldown = false;
            }
            
            if (vectorToPlayer.magnitude > attackRange - attackRangeDeviation && !isOnCooldown) //&& isQueued)
            {
                CancelPathfinding();
                InitiateEnemyState(EnemyState.Attacking);
            }
        }
        else
        {
            InitiatePathfinding();
        }
    }

    // Method will constantly choose random attack if attack is on cooldown
    public virtual void OnAttacking()
    {
        /*
        if (!isQueued && !hasRequested)
        {
            EnemyQueueManager.Instance.RequestToAttack(this);
            hasRequested = true;
            InitiateEnemyState(EnemyState.Repositioning);
            return;
        }
        */

        if (!attackHolder.CanAttack())
        {
            InitiateEnemyState(EnemyState.Chasing);
            return;
        }
        
        if (!isAttacking)
        {
            ChooseNewAttack();

            if (attackHolder.GetActiveAttack().IsOnCooldown())
            {
                attackRange = defaultRepositioningDistance;
                attackRangeDeviation = defaultRepositioningDeviationDistance;
                isOnCooldown = true;
                InitiateEnemyState(EnemyState.Repositioning);
                return;
            }

            Vector3 vectorToPlayer = GetVectorToPlayer();

            if (vectorToPlayer.magnitude > attackRange + attackRangeDeviation)
            {
                InitiateEnemyState(EnemyState.Chasing);
                return;
            }
            else if (vectorToPlayer.magnitude < attackRange - attackRangeDeviation)
            {
                InitiateEnemyState(EnemyState.Repositioning);
                return;
            }

            Aim();

            InitiateAttackState(AttackState.Aiming);

            if (IsAttacking)
            {
                //hasRequested = false;
                chooseNewAttack = true;
            }
        }
    }

    public virtual void OnDodging() { }

    public virtual void OnFleeing() 
    {
        Vector3 vectorToPlayer = GetVectorToPlayer();

        float activationDiff = chaseActivationDistance - vectorToPlayer.magnitude;

        pathfindDestination = transform.position + -vectorToPlayer.normalized * activationDiff;

        if (!isPathfinding)
        {
            InitiatePathfinding();
        }

        if (!IsWithinChasingDistance())
        {
            timeSinceLostPlayer = Time.timeSinceLevelLoad;
            InitiateEnemyState(EnemyState.Idle);
        }
    }

    public virtual void OnStunned() { }

    public virtual void OnDying()
    {
        timer += Time.fixedDeltaTime;

        if (timer > dyingTime)
        {
            Die();
        }
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

    public void ChooseNewAttack()
    {
        if (chooseNewAttack)
        {
            attackHolder.SetRandomActiveAttack();
            attackRange = attackHolder.GetActiveAttack().AttackRange;
            attackRangeDeviation = attackHolder.GetActiveAttack().AttackRangeDeviation;
            chooseNewAttack = false;
        }
    }
}