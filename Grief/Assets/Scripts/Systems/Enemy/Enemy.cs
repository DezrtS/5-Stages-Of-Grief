using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour, IHealth, IEnemy, IAttack, IDodge, IPathfind
{
    private bool isAggro = false;

    // ---------------------------------------------------------------------------------------------------------
    // Enemy Variables
    // ---------------------------------------------------------------------------------------------------------

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
    [SerializeField] private Attack attackTemplate;
    private Attack activeAttack;
    private bool isAttacking;
    private AttackState attackState;

    [Space(10)]
    [Header("Dodging")]
    [SerializeField] private Dodge dodgeTemplate;
    private Dodge dodge;
    private bool isDodging;

    private bool isPathfinding;
    private Vector3 pathfindDestination;
    private NavMeshAgent navMeshAgent;

    // ---------------------------------------------------------------------------------------------------------
    // Interface Implementation Fields
    // ---------------------------------------------------------------------------------------------------------

    public EntityType EntityType { get { return entityType; } }
    public float MaxHealth { get { return maxHealth; } }
    public float Health { get { return health; } }
    public bool IsInvincible { get { return isInvincible; } }
    public EnemyState EnemyState { get { return enemyState; } }
    public Attack ActiveAttack { get { return attackTemplate; } }
    public bool IsAttacking { get { return isAttacking; } }
    public AttackState AttackState { get { return attackState; } }
    public List<EntityType> DamageableEntities { get { return damageableEntities; } }
    public Dodge Dodge { get { return dodgeTemplate; } }
    public bool IsDodging { get { return isDodging; } }
    public bool IsPathfinding { get { return isPathfinding; } }
    public Vector3 PathfindDestination { get { return pathfindDestination; } set { pathfindDestination = value; } }

    // ---------------------------------------------------------------------------------------------------------
    // Default Unity Methods
    // ---------------------------------------------------------------------------------------------------------

    public virtual void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        rigidAgent = GetComponent<RigidAgent>();

        //dodge = dodgeTemplate.Clone(dodge, this, rigidAgent);
        activeAttack = attackTemplate.Clone(activeAttack, this, transform);

        health = maxHealth;
    }

    public virtual void Start()
    {
        player = PlayerController.Instance;

        spawnPosition = transform.position;

        InitiateEnemyState(EnemyState.Idle);
    }

    private void FixedUpdate()
    {
        OnEnemyState(enemyState);

        // Do checks for when to enter each enemy state and initiate them in each OnState
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

        AudioManager.Instance.PlayOneShot(FMODEventsManager.Instance.iceBreak, Vector3.zero);

        health = Mathf.Max(health - damage, 0);

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
        // Destroy Attacks and Dodges on death
        if (isAttacking)
        {
            OnAttackStateCancel(attackState, false);
        }
        if (isDodging)
        {
            OnDodgeCancel(false);
        }

        if (isAggro)
        {
            player.targeting--;
            if (player.targeting == 0)
            {
                AudioManager.Instance.SetAmbienceParameter("combat_intensity", 0);
                //Debug.Log("Setting Combat OFF");
            }
        }

        EventInstance dialogue = PlayerController.Instance.dialogue;

        if (Random.Range(0, 2) == 1)
        {
            dialogue.getPlaybackState(out PLAYBACK_STATE playbackState);
            if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
            {
                dialogue.setParameterByName("dialogue_option", 2);
                dialogue.start();
            }
        }

        activeAttack.DestroyClone();
        //dodge.DestroyClone();

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
                if (isAggro)
                {
                    player.targeting--;
                    if (player.targeting == 0)
                    {
                        AudioManager.Instance.SetAmbienceParameter("combat_intensity", 0);
                        //Debug.Log("Setting Combat OFF");
                    }
                }

                isAggro = false;
                break;
            case EnemyState.Patrolling:
                pathfindDestination = spawnPosition;
                InitiatePathfinding();
                break;
            case EnemyState.Chasing:
                if (!isAggro)
                {
                    player.targeting++;

                    AudioManager.Instance.PlayOneShot(FMODEventsManager.Instance.snarl, transform.position);

                    if (player.targeting == 1)
                    {
                        //Debug.Log("Setting Combat ON");
                        AudioManager.Instance.SetAmbienceParameter("combat_intensity", 1);
                    }
                }

                isAggro = true;
                break;
            case EnemyState.Attacking:
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
            case EnemyState.Attacking:
                OnAttacking();
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
            case EnemyState.Attacking:

                break;
            case EnemyState.Fleeing:

                break;
            case EnemyState.Stunned:

                break;
            case EnemyState.Dead:

                break;
        }
    }

    public virtual void SwitchActiveAttack(Attack activeAttack)
    {
        if (CanSwitchActiveAttack(activeAttack))
        {
            isAttacking = false;
            this.activeAttack = activeAttack;
        }
    }

    public virtual bool CanSwitchActiveAttack(Attack activeAttack)
    {
        if (this.activeAttack == activeAttack)
        {
            return false;
        }

        return attackState == AttackState.Idle;
    }

    public virtual void TransferToAttackState(AttackState attackState)
    {
        this.attackState = attackState;
    }

    public virtual void InitiateAttackState(AttackState attackState)
    {
        if (CanInitiateAttackState(attackState, activeAttack.AttackId))
        {
            activeAttack.TransferToAttackState(attackState);
        }
    }

    public virtual bool CanInitiateAttackState(AttackState attackState, string attackId)
    {
        // Specific requirements to the class rather than the attack

        if (activeAttack == null)
        {
            Debug.LogWarning($"{name} Does Not Have An Attack");
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
        if (attackState != AttackState.Idle)
        {
            isAttacking = true;
        }

        if (attackState == AttackState.Aiming || attackState == AttackState.Attacking)
        {
            rigidAgent.SetAllowMovementInput(false);
        }

        if (attackState == AttackState.ChargingUp)
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
        else if (attackState == AttackState.Idle)
        {
            rigidAgent.SetAllowMovementInput(true);
            rigidAgent.SetAllowRotationInput(true);

            isAttacking = false;
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
            activeAttack.OnAttackStateCancel(attackState, true);
        }

        this.attackState = AttackState.Idle;
        rigidAgent.SetAllowMovementInput(true);
        rigidAgent.SetAllowRotationInput(true);
        isAttacking = false;
    }

    public virtual void InitiateDodge()
    {
        if (CanInitiateDodge())
        {

        }
    }

    public virtual bool CanInitiateDodge()
    {
        // Specific requirements to the class rather than the dodge

        if (dodge == null)
        {
            Debug.LogWarning($"{name} Does Not Have An Dodge");
            return false;
        }

        return (!IsDodging && attackState == AttackState.Idle && !isPathfinding);
    }

    public virtual void OnDodgeStart()
    {
        // Actions to do when the dodge has first started
        isDodging = true;
        rigidAgent.SetAllowMovementInput(false);
        rigidAgent.SetAllowRotationInput(false);
    }

    public virtual void OnDodge()
    {
        // Actions to do while the dodge is happening
    }

    public virtual void OnDodgeEnd()
    {
        // Actions to do when the dodge has ended
        isDodging = false;
        rigidAgent.SetAllowMovementInput(true);
        rigidAgent.SetAllowRotationInput(true);
    }

    public virtual void OnDodgeCancel(bool otherHasCancelled)
    {
        // Actions to do when the state is cancelled
        if (!otherHasCancelled)
        {
            dodge.OnDodgeCancel(true);
        }

        isDodging = false;
        rigidAgent.SetAllowMovementInput(true);
        rigidAgent.SetAllowRotationInput(true);
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
            Vector3 wanderDestination = transform.position + new Vector3(UnityEngine.Random.Range(-wanderRange, wanderRange), 0, UnityEngine.Random.Range(-wanderRange, wanderRange));

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
        return chance <= UnityEngine.Random.Range(0, 100);
    }
}