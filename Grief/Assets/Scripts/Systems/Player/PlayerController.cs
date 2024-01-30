using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerController : Singleton<PlayerController>, IHealth, IMove, IAttack, IDodge, IPathfind, IStatusEffectTarget, IAnimate
{
    public EventInstance dialogue;

    // ---------------------------------------------------------------------------------------------------------
    // Player Variables
    // ---------------------------------------------------------------------------------------------------------

    [Header("Pathfinding Debugging")]
    [SerializeField] private GameObject pathfindObject;

    [Header("Camera Panning")]
    [SerializeField] private float panStrength = 5;
    [Range(0.1f, 100)]
    [SerializeField] private float panTimeMultiplier = 10;

    private Transform playerTransform;
    private CharAgent charAgent;

    private PlayerLook playerLook;

    private PlayerInputControls playerInputControls;
    private InputAction leftJoystick;
    private InputAction rightJoystick;
    private InputAction dPad;
    private Vector2 lastDPadInput = Vector2.up;

    private bool useMouseForRotation;

    [SerializeField] private bool useAimAssist = false;
    private bool assistAim;

    private bool wantToAttack = false;


    // ---------------------------------------------------------------------------------------------------------
    // Interface Related Variables
    // ---------------------------------------------------------------------------------------------------------

    [Space(10)]
    [Header("Health")]
    [SerializeField] private float maxHealth;
    private readonly EntityType entityType = EntityType.Player;
    private float health;
    private bool isInvincible;

    [Space(10)]
    [Header("Attacking")]
    [SerializeField] private List<EntityType> damageableEntities;
    private AttackHolder attackHolder;
    private GameObject particleEffectHolder;
    private AttackState attackState;
    private bool isAttacking;

    private bool queueAttack;

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
    public AttackHolder AttackHolder { get { return attackHolder; } }
    public GameObject ParticleEffectHolder { get { return particleEffectHolder; } }
    public bool IsAttacking { get { return isAttacking; } }
    public AttackState AttackState { get { return attackState; } }
    public List<EntityType> DamageableEntities { get { return damageableEntities; } }
    public DodgeHolder DodgeHolder {  get { return dodgeHolder; } }
    public bool IsDodging { get { return isDodging; } }
    public DodgeState DodgeState {  get { return dodgeState; } }
    public bool IsPathfinding { get { return isPathfinding; } }
    public Vector3 PathfindDestination { get { return pathfindDestination; } set { pathfindDestination = value; } }
    public StatusEffectHolder StatusEffectHolder { get { return statusEffectHolder; } }

    // ---------------------------------------------------------------------------------------------------------
    // Class Events
    // ---------------------------------------------------------------------------------------------------------

    public delegate void PlayerHealthEventHandler(float health);

    public event PlayerHealthEventHandler OnPlayerHealthEvent;

    public delegate void AbilitySelectEventHandler(int ability);

    public event AbilitySelectEventHandler OnAbilitySelectEvent;

    public delegate void AttackEventHandler(string attackId);

    public event AttackEventHandler OnAttackEvent;


    // ---------------------------------------------------------------------------------------------------------
    // Default Unity Methods
    // ---------------------------------------------------------------------------------------------------------

    protected override void Awake()
    {
        base.Awake();

        playerTransform = transform;
        navMeshAgent = GetComponent<NavMeshAgent>();
        charAgent = GetComponent<CharAgent>();
        playerLook = transform.AddComponent<PlayerLook>();


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

    private void Start()
    {
        particleEffectHolder = Instantiate(GameManager.Instance.EmptyGameObject, transform);
        particleEffectHolder.name = $"{name}'s Particle Effect Holder";

        dialogue = AudioManager.Instance.CreateInstance(FMODEventsManager.Instance.dialogue);
        CameraManager.Instance.TransferCameraTo(transform);
    }

    private void FixedUpdate()
    {
        if (wantToAttack)
        {
            if (!attackHolder.GetActiveAttack().IsOnCooldown())
            {
                InitiateAttackState(AttackState.ChargingUp);
                StopAllCoroutines();
                wantToAttack = false;
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            InitiateDodgeState(DodgeState.Aiming);
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (dodgeState == DodgeState.Aiming)
            {
                InitiateDodgeState(DodgeState.ChargingUp);
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && !isAttacking)
        {
            useMouseForRotation = true;

            InitiateAttackState(AttackState.Aiming);
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (attackState == AttackState.Aiming)
            {
                InitiateAttackState(AttackState.ChargingUp);
            }
        }
    }

    private void LateUpdate()
    {
        playerLook.PanTowards(rightJoystick.ReadValue<Vector2>(), panStrength, panTimeMultiplier);
    }

    private void OnEnable()
    {
        playerInputControls ??= new PlayerInputControls();

        leftJoystick = playerInputControls.Player.Movement;
        leftJoystick.Enable();

        rightJoystick = playerInputControls.Player.Look;
        rightJoystick.Enable();

        playerInputControls.Player.Action.performed += OnActionPerformed;
        playerInputControls.Player.Action.canceled += OnActionCancelled;
        playerInputControls.Player.Action.Enable();

        dPad = playerInputControls.Player.AbilitySelection;
        dPad.performed += OnSelectAbility;
        dPad.Enable();
    }

    private void OnActionPerformed(InputAction.CallbackContext obj)
    {
        if (GameManager.Instance.IsPaused)
        {
            return;
        }

        useMouseForRotation = false;

        string buttonName = obj.action.activeControl.name;

        switch (buttonName)
        {
            case "buttonNorth":

                if (lastDPadInput == Vector2.up)
                {
                    AttackHolder.SetActiveAttack(1);
                }
                else if (lastDPadInput == Vector2.left)
                {
                    AttackHolder.SetActiveAttack(2);
                }
                else if (lastDPadInput == Vector2.right)
                {
                    AttackHolder.SetActiveAttack(3);
                }
                else if (lastDPadInput == Vector2.down)
                {
                    AttackHolder.SetActiveAttack(1);
                } 
                else
                {
                    return;
                }

                InitiateAttackState(AttackState.Aiming);

                //pathfindDestination = (new Vector3(pathfindObject.transform.position.x, transform.position.y, pathfindObject.transform.position.z));
                //InitiatePathfinding();
                return;
            case "buttonEast":

                return;
            case "buttonSouth":
                InitiateDodgeState(DodgeState.Aiming);
                return;
            case "buttonWest":
                AttackHolder.SetActiveAttack(0);

                if (AttackHolder.CanAttack())
                {
                    if (AttackHolder.GetActiveAttack().IsOnCooldown())
                    {
                        StartCoroutine(AttackPressTimer());
                    }
                    else
                    {
                        InitiateAttackState(AttackState.Aiming);
                    }
                }
                return;
            default:
                return;
        }
    }

    private void OnActionCancelled(InputAction.CallbackContext obj)
    {
        string buttonName = obj.action.activeControl.name;

        switch (buttonName)
        {
            case "buttonNorth":
                if (attackState == AttackState.Aiming)
                {
                    InitiateAttackState(AttackState.ChargingUp);
                }
                return;
            case "buttonEast":

                return;
            case "buttonSouth":
                if (dodgeState == DodgeState.Aiming)
                {
                    InitiateDodgeState(DodgeState.ChargingUp);
                }
                return;
            case "buttonWest":
                if (attackState == AttackState.Aiming)
                {

                    //InitiateAttackState(AttackState.ChargingUp);
                }
                return;
            default:
                return;
        }
    }

    private void OnSelectAbility(InputAction.CallbackContext obj)
    {
        if (GameManager.Instance.IsPaused)
        {
            return;
        }

        Vector2 dPadInput = dPad.ReadValue<Vector2>();
        int selectedAbility = -1;

        if (dPadInput == Vector2.up)
        {
            selectedAbility = 0;
        }
        else if (dPadInput == Vector2.left)
        {
            selectedAbility = 1;
        }
        else if (dPadInput == Vector2.right)
        {
            selectedAbility = 2;
        }
        else if (dPadInput == Vector2.down)
        {
            //selectedAbility = 3;
        }

        OnAbilitySelectEvent.Invoke(selectedAbility);

        lastDPadInput = dPadInput;
    }

    private void OnDisable()
    {
        leftJoystick.Disable();
        rightJoystick.Disable();

        playerInputControls.Player.Action.performed -= OnActionPerformed;
        playerInputControls.Player.Action.canceled -= OnActionCancelled;
        playerInputControls.Player.Action.Disable();

        dPad.performed -= OnSelectAbility;
        dPad.Disable();
    }

    // ---------------------------------------------------------------------------------------------------------
    // Interface Implementation Methods
    // ---------------------------------------------------------------------------------------------------------

    public void Damage(float damage)
    {
        if (isInvincible)
        {
            return;
        }

        health = Mathf.Max(health - damage, 0);
        OnAnimationStart(AnimationEvent.Hurt, "");

        if (Random.Range(0, 10) == 1)
        {
            dialogue.getPlaybackState(out PLAYBACK_STATE playbackState);
            if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
            {
                dialogue.setParameterByName("dialogue_option", 0);
                dialogue.start();
            }
        }
        else
        {
            AudioManager.Instance.PlayOneShot(FMODEventsManager.Instance.playerHurt, transform.position);
        }

        //AudioManager.Instance.PlayOneShot(FMODEventsManager.Instance.hit, transform.position);
        CameraManager.Instance.Shake(6, 0.2f);

        EffectManager.Instance.Flash(transform);

        OnPlayerHealthEvent?.Invoke(health);

        if (health == 0)
        {
            Die();
        }
    }

    public void Heal(float healing)
    {
        health = Mathf.Min(health + healing, maxHealth);

        OnPlayerHealthEvent?.Invoke(health);
    }

    public void Die()
    {
        if (isAttacking)
        {
            OnAttackStateCancel(attackState, false);
        }
        if (isDodging)
        {
            OnDodgeStateCancel(dodgeState, false);
        }

        OnAnimationStart(AnimationEvent.Die, "");

        statusEffectHolder.ClearStatusEffects();

        CancelPathfinding();

        if (CheckpointManager.Instance.GetActiveCheckpoint() != null)
        {
            charAgent.Teleport(CheckpointManager.Instance.GetActiveCheckpoint().transform.position);
        }
        else
        {
            charAgent.Teleport(Vector3.up);
        }

        charAgent.SetVelocity(Vector3.zero);
        charAgent.SetRotation(Quaternion.Inverse(MovementController.MovementAxis) * Vector3.right);

        Heal(MaxHealth);

        isInvincible = true;

        StartCoroutine(InvincibilityTimer());

        //EnemySpawning.Instance.KillAllEnemies();
        //EnemySpawning.Instance.ResetSpawner();
    }

    public Vector3 GetMovementInput()
    {
        return leftJoystick.ReadValue<Vector2>();
    }

    public Vector3 GetRotationInput()
    {
        if (useMouseForRotation)
        {
            return ((Vector2)Input.mousePosition - new Vector2(Screen.width, Screen.height) * 0.5f).normalized;
        }

        return GetMovementInput();
    }

    public void TransferToAttackState(AttackState attackState)
    {
        this.attackState = attackState;
    }

    public void InitiateAttackState(AttackState attackState)
    {
        if (CanInitiateAttackState(attackState, attackHolder.GetAttackId()))
        {
            attackHolder.GetActiveAttack().TransferToAttackState(attackState);
        }
    }

    public bool CanInitiateAttackState(AttackState attackState, string attackId)
    {
        if (!attackHolder.CanAttack())
        {
            return false;
        }
        else if (isDodging)
        {
            queueAttack = true;
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

    public void OnAttackStateStart(AttackState attackState, string attackId)
    {
        // Can be Simplified

        if (attackState != AttackState.Idle)
        {
            isAttacking = true;
        }

        // Actions to do when the state has first started
        if (attackState == AttackState.Aiming)
        {
            charAgent.SetAllowMovementInput(false);

            if (useAimAssist)
            {
                if (attackId == "ice_shard")
                {
                    assistAim = true;
                }
            }
        }
        else if (attackState == AttackState.ChargingUp)
        {
            charAgent.SetAllowMovementInput(false);
            charAgent.SetAllowRotationInput(false);

            if (Random.Range(0, 10) == 1)
            {
                dialogue.getPlaybackState(out PLAYBACK_STATE playbackState);
                if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
                {
                    dialogue.setParameterByName("dialogue_option", 1);
                    dialogue.start();
                }
            }

            //AudioManager.Instance.PlayOneShot(FMODEventsManager.Instance.playerSwing, transform.position);
            //playerAnimation.Swing();
        }
        else if (attackState == AttackState.Attacking)
        {
            OnAttackEvent?.Invoke(attackId);
            Aim();

            if (assistAim)
            {
                List<Enemy> enemies = EnemyManager.Instance.Enemies;

                Vector3 dir;
                float closestAngle = 30f;
                Vector3 closestDir = transform.forward;

                for (int i = 0; i < enemies.Count; i++)
                {
                    dir = enemies[i].transform.position - transform.position;
                    dir.y = 0;

                    if (dir.magnitude < 45)
                    {
                        float angle = Vector3.Angle(transform.forward, dir.normalized);
                        //Debug.Log(angle);
                        if (angle <= closestAngle)
                        {
                            closestAngle = angle;
                            closestDir = dir.normalized;
                        }
                    }
                }

                charAgent.SetRotation(Quaternion.Inverse(MovementController.MovementAxis) * closestDir);
                assistAim = false;
            }

            charAgent.SetAllowMovementInput(false);
            charAgent.SetAllowRotationInput(false);
        } 
        else if (attackState == AttackState.CoolingDown)
        {
            charAgent.SetAllowMovementInput(false);
            charAgent.SetAllowRotationInput(false);
        }
        else if (attackState == AttackState.Idle)
        {
            charAgent.SetAllowMovementInput(true);
            charAgent.SetAllowRotationInput(true);

            isAttacking = false;
        }
    }

    public void OnAttackState(AttackState attackState)
    {
        // Actions to do while the state is happening
        if (attackState == AttackState.Aiming)
        {
            Aim();
        }
    }

    public void OnAttackStateEnd(AttackState attackState)
    {
        // Actions to do when the state has ended
        charAgent.SetAllowMovementInput(true);
        charAgent.SetAllowRotationInput(true);
    }

    public void OnAttackStateCancel(AttackState attackState, bool otherHasCancelled)
    {
        // Actions to do when the state is cancelled
        if (!otherHasCancelled)
        {
            if (attackState ==  AttackState.Idle)
            {
                // Cancelled On Idle
                return;
            }

            attackHolder.GetActiveAttack().OnAttackStateCancel(attackState, true);
        }

        this.attackState = AttackState.Idle;
        charAgent.SetAllowMovementInput(true);
        charAgent.SetAllowRotationInput(true);
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
            charAgent.SetAllowMovementInput(true);
            charAgent.SetAllowRotationInput(true);

            isDodging = false;

            if (queueAttack)
            {
                InitiateAttackState(AttackState.Aiming);
                InitiateAttackState(AttackState.ChargingUp);
                queueAttack = false;
            }
            return;
        }
        else
        {
            isDodging = true;
        }

        if (dodgeState == DodgeState.Aiming)
        {
            charAgent.SetAllowMovementInput(false);
        }
        else if (dodgeState == DodgeState.ChargingUp)
        {
            charAgent.SetAllowMovementInput(false);
            charAgent.SetAllowRotationInput(false);
        }
        else if (dodgeState == DodgeState.Dodging)
        {
            // Change this out to use timer on FixedUpdate()
            StartCoroutine(InvincibilityTimer());
            Aim();
            charAgent.SetAllowMovementInput(false);
            charAgent.SetAllowRotationInput(false);
        }
        else if (dodgeState == DodgeState.CoolingDown)
        {
            charAgent.SetAllowMovementInput(false);
            charAgent.SetAllowRotationInput(false);
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
        charAgent.SetAllowMovementInput(true);
        charAgent.SetAllowRotationInput(true);
    }

    public void OnDodgeStateCancel(DodgeState dodgeState, bool otherHasCancelled)
    {
        // Actions to do when the state is cancelled
        if (!otherHasCancelled)
        {
            if (dodgeState == DodgeState.Idle)
            {
                // Cancelled On Idle
                return;
            }

            dodgeHolder.GetActiveDodge().OnDodgeStateCancel(dodgeState, true);
        }

        this.dodgeState = DodgeState.Idle;
        charAgent.SetAllowMovementInput(true);
        charAgent.SetAllowRotationInput(true);
        isDodging = false;
    }

    public void TransferToPathfindingState(bool isPathfinding)
    {
        this.isPathfinding = isPathfinding;
    }

    public void InitiatePathfinding()
    {
        if (CanInitiatePathfinding())
        {
            charAgent.InitiatePathfinding(transform, pathfindDestination);
        }
    }

    public bool CanInitiatePathfinding()
    {
        return (!isDodging && attackState == AttackState.Idle);
    }

    public void OnPathfinding()
    {

    }

    public void CancelPathfinding()
    {
        charAgent.StopPathfinding();
    }

    public void OnAnimationStart(AnimationEvent animationEvent, string animationId)
    {
        if (!canAnimate)
        {
            return;
        }

        // Put check to see if animator has parameter.

        switch (animationEvent)
        {
            case AnimationEvent.AimAttack:
                //animator.SetTrigger("AimAttack");
                break;
            case AnimationEvent.AimDodge:
                //animator.SetTrigger("AimDodge");
                break;
            case AnimationEvent.AimAttackCancel:
                //animator.SetTrigger("AimAttackCancel");
                break;
            case AnimationEvent.AimDodgeCancel:
                //animator.SetTrigger("AimDodgeCancel");
                break;
            case AnimationEvent.Attack:
                animator.SetTrigger("Attack");
                break;
            case AnimationEvent.Dodge:
                animator.SetTrigger("Dodge");
                break;
            case AnimationEvent.Walk:
                animator.SetBool("IsRunning", false);
                animator.SetBool("IsWalking", true);
                break;
            case AnimationEvent.Run:
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsRunning", true);
                break;
            case AnimationEvent.Stand:
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsRunning", false);
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
    // Class Specific Methods
    // ---------------------------------------------------------------------------------------------------------

    public void Aim()
    {
        Vector3 targetDirection = GetRotationInput();

        if (targetDirection.magnitude > 0)
        {
            charAgent.SetRotation(targetDirection);
        }
    }

    // ---------------------------------------------------------------------------------------------------------
    // Coroutines
    // ---------------------------------------------------------------------------------------------------------

    public IEnumerator InvincibilityTimer()
    {
        isInvincible = true;
        //Debug.Log("Started Invincibility");
        yield return new WaitForSeconds(0.2f);
        //Debug.Log("Ended Invincibility");
        isInvincible = false;
    }

    public IEnumerator AttackPressTimer()
    {
        wantToAttack = true;
        yield return new WaitForSeconds(0.2f);
        wantToAttack = false;
    }
}