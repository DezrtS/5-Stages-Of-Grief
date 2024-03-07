using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerController : Singleton<PlayerController>, IHealth, IMove, IAttack, IDodge, IPathfind, IStatusEffectTarget, IAnimate
{
    //public EventInstance dialogue;
    private bool useHeavyAttack;

    // ---------------------------------------------------------------------------------------------------------
    // PlayerController Class Variables
    // ---------------------------------------------------------------------------------------------------------

    [Header("Pathfinding Debugging")]
    [SerializeField] private GameObject pathfindObject;

    [Header("Camera Panning")]
    [SerializeField] private float panStrength = 5;
    [Range(0.1f, 100)]
    [SerializeField] private float panTimeMultiplier = 10;

    [Space(10)]
    [Header("Aim Assist")]
    [SerializeField] private bool useAimAssist = false;
    [SerializeField] private float aimAssistMaxAngle = 20f;
    [SerializeField] private float aimAssistMaxRange = 45f;

    private Transform playerTransform;
    private CharAgent charAgent;

    private PlayerLook playerLook;

    private PlayerInputControls playerInputControls;
    private InputAction leftJoystick;
    private InputAction rightJoystick;
    private InputAction dPad;
    private Vector2 lastDPadInput = Vector2.up;

    private bool useMouseForRotation;

    private bool assistAim;

    private bool queueAttack;
    private bool queueDodge;
    private float queueAttackTimer = 0;
    private float queueDodgeTimer = 0;

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
    [SerializeField] private float attackCoyoteTime = 0.2f;
    [SerializeField] private List<EntityType> damageableEntities;
    private AttackHolder attackHolder;
    private GameObject particleEffectHolder;
    private AttackState attackState;
    private bool isAttacking;

    [Space(10)]
    [Header("Dodging")]
    [SerializeField] private float dodgeCoyoteTime = 0.2f;
    private DodgeHolder dodgeHolder;
    private DodgeState dodgeState;
    private bool isDodging;

    private bool isPathfinding;
    private Vector3 pathfindDestination;
    private NavMeshAgent navMeshAgent;

    private StatusEffectHolder statusEffectHolder;

    [Space(10)]
    [Header("Animating")]
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
    // PlayerController Class Events
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
        // Call the base awake function from the Singleton class.
        base.Awake();

        // Assign/create the needed variables for this class to work.
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

        // Determine if this class can animate .
        if (animator == null)
        {
            canAnimate = false;
        }

        health = maxHealth;
    }

    private void Start()
    {
        // Create the particle effect holder for this class.
        particleEffectHolder = Instantiate(GameManager.Instance.EmptyGameObject, transform);
        particleEffectHolder.name = $"{name}'s Particle Effect Holder";

        // Create an audio instance for playing dialogue
        //dialogue = AudioManager.Instance.CreateInstance(FMODEventsManager.Instance.dialogue);
        // Call the CameraManager singleton to transfer the camera to this transform.
        CameraManager.Instance.TransferCameraTo(transform);
    }

    private void FixedUpdate()
    {
        // Check to see if the player has queued a dodge, and when the dodge is no longer on a cooldown,
        // the player is not attacking, and is not pathfinding, reset the dodge queue timer and activate a dodge.
        if (queueDodge)
        {
            queueDodgeTimer -= Time.fixedDeltaTime;

            if (!dodgeHolder.GetActiveDodge().IsOnCooldown() && !isAttacking && !isPathfinding)
            {
                queueDodge = false;
                queueDodgeTimer = 0;
                InitiateDodgeState(DodgeState.Aiming);
                return;
            }

            if (queueDodgeTimer <= 0)
            {
                queueDodge = false;
            }
        }

        // Works in the same way as with queuing a dodge.
        if (queueAttack)
        {
            queueAttackTimer -= Time.fixedDeltaTime;

            if (!attackHolder.GetActiveAttack().IsOnCooldown() && !isDodging && !isPathfinding)
            {
                queueAttack = false;
                queueAttackTimer = 0;
                InitiateAttackState(AttackState.Aiming);
                InitiateAttackState(AttackState.ChargingUp);
                return;
            }

            if (queueAttackTimer <= 0)
            {
                queueAttack = false;
            }
        }
    }

    private void Update()
    {
        // Get input for various debugging purposes (Has to be properly implemented using the new input system later).
        if (Input.GetKeyDown(KeyCode.Mouse0) && !isAttacking)
        {
            useMouseForRotation = true;
            AttackHolder.SetActiveAttack(0);
            InitiateAttackState(AttackState.Aiming);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            useMouseForRotation = true;
            SetAbility();
            InitiateAttackState(AttackState.Aiming);
        }

        if (Input.GetKeyUp(KeyCode.Mouse0) || Input.GetKeyUp(KeyCode.F))
        {
            if (attackState == AttackState.Aiming)
            {
                InitiateAttackState(AttackState.ChargingUp);
            }
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            useHeavyAttack = !useHeavyAttack;
        }
    }

    private void LateUpdate()
    {
        // Pan the camera towards the direction of the right joystick.
        playerLook.PanTowards(rightJoystick.ReadValue<Vector2>(), panStrength, panTimeMultiplier);
    }

    private void OnEnable()
    {
        // Enables the player input system controls and connects various input events to some of the methods in this class.
        playerInputControls ??= new PlayerInputControls();

        leftJoystick = playerInputControls.Player.Movement;
        leftJoystick.Enable();

        rightJoystick = playerInputControls.Player.Look;
        rightJoystick.Enable();

        //playerInputControls.Player.Action.started += OnActionStarted;
        playerInputControls.Player.Action.performed += OnActionPerformed;
        playerInputControls.Player.Action.canceled += OnActionCancelled;
        playerInputControls.Player.Action.Enable();

        dPad = playerInputControls.Player.AbilitySelection;
        dPad.performed += OnSelectAbility;
        dPad.Enable();
    }

    /*
    private void OnActionStarted(InputAction.CallbackContext obj)
    {
        if (GameManager.Instance.IsPaused)
        {
            return;
        }

        useMouseForRotation = false;

        string buttonName = obj.action.activeControl.name;

        switch (buttonName)
        {
            case "rightTrigger":

                return;
            case "buttonNorth":

                return;
            case "buttonEast":

                return;
            case "space":
            case "buttonSouth":

                return;
            case "buttonWest":
                AttackHolder.SetActiveAttack(0);
                InitiateAttackState(AttackState.Aiming);
                return;
            default:
                // Default case: Do nothing for unrecognized controls.
                return;
        }
    }
    */

    /// <summary>
    /// This method is a callback function for player input actions. It is invoked when a specific input action is performed.
    /// </summary>
    /// <param name="obj">The InputAction.CallbackContext object containing information about the performed action.</param>
    private void OnActionPerformed(InputAction.CallbackContext obj)
    {
        // Check if the game is paused; if paused, do nothing in response to the input action.
        if (GameManager.Instance.IsPaused)
        {
            return;
        }

        // Disable mouse rotation because mouse input caannot currently trigger this function so if mouse
        // rotation was enabled, the user must have switched to a controller.
        useMouseForRotation = false;

        // Get the name of the button associated with the performed action.
        string buttonName = obj.action.activeControl.name;

        // Process the input based on the button's name.
        switch (buttonName)
        {
            case "rightTrigger":
                // Set the ability and initiate the attack aiming state.
                SetAbility();
                InitiateAttackState(AttackState.Aiming);
                return;
            case "buttonNorth":
                // Check if heavy attack is enabled and set the corresponding heavy attack, or just set the ability from the default.
                // This is meant for testing purposes.
                if (useHeavyAttack)
                {
                    attackHolder.SetActiveAttack(4);
                }
                else
                {
                    SetAbility();
                }
                InitiateAttackState(AttackState.Aiming);
                return;
            case "buttonEast":
                // Specific action for the "buttonEast" control (Currently has no use).
                return;
            case "space":
            case "buttonSouth":
                // Initiate the dodge aiming state.
                InitiateDodgeState(DodgeState.Aiming);
                return;
            case "buttonWest":
                // Set the activate attack to the default attack and initiate the attack aiming state.
                //OnAttackStateCancel(attackState, false);
                //AttackHolder.SetActiveAttack(4);
                AttackHolder.SetActiveAttack(0);
                InitiateAttackState(AttackState.Aiming);
                return;
            default:
                // Default case: Do nothing for unrecognized controls.
                return;
        }
    }

    private void OnActionCancelled(InputAction.CallbackContext obj)
    {
        string buttonName = obj.action.activeControl.name;

        switch (buttonName)
        {
            case "rightTrigger":
            case "buttonNorth":
                if (attackState == AttackState.Aiming)
                {
                    InitiateAttackState(AttackState.ChargingUp);
                }
                return;
            case "buttonEast":
                return;
            case "buttonSouth":
                return;
            case "buttonWest":
                /*
                if (attackState == AttackState.Aiming)
                {
                    InitiateAttackState(AttackState.ChargingUp);
                }
                */
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

        OnAbilitySelectEvent?.Invoke(selectedAbility);

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

        //if (Random.Range(0, 10) == 1)
        //{
        //    dialogue.getPlaybackState(out PLAYBACK_STATE playbackState);
        //    if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
        //    {
        //        dialogue.setParameterByName("dialogue_option", 0);
        //        dialogue.start();
        //    }
        //}
        AudioManager.Instance.PlayOneShot(FMODEventsManager.Instance.PlayerHurt, transform.position);

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
        else if (this.attackState == attackState)
        {
            return false;
        }
        else if (attackState == AttackState.Aiming)
        {
            if (isAttacking)
            {
                QueueAttack();
                return false;
            }
            else if (isDodging)
            {
                QueueAttack();
                return false;
            }
            else if (isPathfinding)
            {
                QueueAttack();
                return false;
            }
            else if (attackHolder.GetActiveAttack().IsOnCooldown())
            {
                QueueAttack();
                return false;
            }
        }

        return true;
    }

    public void OnAttackStateStart(AttackState attackState, string attackId)
    {
        if (attackState == AttackState.Idle)
        {
            charAgent.SetAllowMovementInput(true);
            charAgent.SetAllowRotationInput(true);
            isAttacking = false;
            return;
        } 
        else 
        {
            isAttacking = true;
        }

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
        }
        else if (attackState == AttackState.Attacking)
        {
            OnAttackEvent?.Invoke(attackId);
            Aim();

            if (assistAim)
            {
                List<Enemy> enemies = EnemyManager.Instance.Enemies;

                Vector3 dir;
                float closestAngle = aimAssistMaxAngle;
                Vector3 closestDir = transform.forward;

                for (int i = 0; i < enemies.Count; i++)
                {
                    dir = enemies[i].transform.position - transform.position;
                    dir.y = 0;

                    if (dir.magnitude < aimAssistMaxRange)
                    {
                        float angle = Vector3.Angle(transform.forward, dir.normalized);
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
        else if (this.dodgeState == dodgeState)
        {
            return false;
        }
        else if (dodgeState == DodgeState.Aiming)
        {
            if (isDodging)
            {
                QueueDodge();
                return false;
            }
            else if (isAttacking)
            {
                QueueDodge();
                return false;
            }
            else if (isPathfinding)
            {
                QueueDodge();
                return false;
            } 
            else if (dodgeHolder.GetActiveDodge().IsOnCooldown())
            {
                QueueDodge();
                return false;
            }
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

    public void SetAbility()
    {
        AttackHolder.SetActiveAttack(1);

        /*
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
        */
    }

    public void QueueDodge()
    {
        if (!queueDodge)
        {
            queueDodgeTimer = dodgeCoyoteTime;
            queueDodge = true;
            queueAttack = false;
        }
    }

    public void QueueAttack()
    {
        if (!queueDodge && !queueAttack)
        {
            queueAttackTimer = attackCoyoteTime;
            queueAttack = true;
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
}