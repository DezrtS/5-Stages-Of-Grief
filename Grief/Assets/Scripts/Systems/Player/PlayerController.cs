using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerController : Singleton<PlayerController>, IHealth, IMove, IAttack, IDodge, IPathfind
{
    [SerializeField] private PlayerAnimation playerAnimation;

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
    [SerializeField] private Attack basicAttackTemplate;
    [SerializeField] private Attack iceShardAttackTemplate;
    [SerializeField] private Attack firePunchAttackTemplate;
    [SerializeField] private Attack teleportingKunaiAttackTemplate;
    [SerializeField] private Attack lightBeamAttackTemplate;
    private Attack basicAttack;
    private Attack iceShardAttack;
    private Attack firePunchAttack;
    private Attack teleportingKunaiAttack;
    private Attack lightBeamAttack;

    private Attack activeAttack;
    private AttackState attackState;
    private bool isAttacking;

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
    public Attack ActiveAttack { get { return activeAttack; } }
    public bool IsAttacking { get { return isAttacking; } }
    public AttackState AttackState { get { return attackState; } }
    public List<EntityType> DamageableEntities { get { return damageableEntities; } }
    public Dodge Dodge { get { return dodgeTemplate; } }
    public bool IsDodging { get { return isDodging; } }
    public bool IsPathfinding { get { return isPathfinding; } }
    public Vector3 PathfindDestination { get { return pathfindDestination; } set { pathfindDestination = value; } }

    // ---------------------------------------------------------------------------------------------------------
    // Class Events
    // ---------------------------------------------------------------------------------------------------------

    public delegate void PlayerHealthEventHandler(float health);

    public event PlayerHealthEventHandler OnPlayerHealthEvent;

    public delegate void AbilitySelectEventHandler(int ability);

    public event AbilitySelectEventHandler OnAbilitySelectEvent;

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

        dodge = dodgeTemplate.Clone(dodge, this, charAgent);

        basicAttack = basicAttackTemplate.Clone(basicAttack, this, transform);
        iceShardAttack = iceShardAttackTemplate.Clone(iceShardAttack, this, transform);
        firePunchAttack = firePunchAttackTemplate.Clone(firePunchAttack, this, transform);
        teleportingKunaiAttack = teleportingKunaiAttackTemplate.Clone(teleportingKunaiAttack, this, transform);
        lightBeamAttack = lightBeamAttackTemplate.Clone(lightBeamAttack, this, transform);


        activeAttack = basicAttack;

        health = maxHealth;
    }

    private void Start()
    {
        CameraManager.Instance.TransferCameraTo(transform);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            InitiateDodge();
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

    private void FixedUpdate()
    {

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
                    SwitchActiveAttack(iceShardAttack);
                }
                else if (lastDPadInput == Vector2.left)
                {
                    SwitchActiveAttack(firePunchAttack);
                }
                else if (lastDPadInput == Vector2.right)
                {
                    SwitchActiveAttack(teleportingKunaiAttack);
                }
                else if (lastDPadInput == Vector2.down)
                {
                    //SwitchActiveAttack(lightBeamAttack);
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
                InitiateDodge();
                return;
            case "buttonWest":
                SwitchActiveAttack(basicAttack);

                InitiateAttackState(AttackState.Aiming);
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

                return;
            case "buttonWest":
                if (attackState == AttackState.Aiming)
                {
                    InitiateAttackState(AttackState.ChargingUp);
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

        OnPlayerHealthEvent(health);

        if (health == 0)
        {
            Die();
        }
    }

    public void Heal(float healing)
    {
        health = Mathf.Min(health + healing, maxHealth);

        OnPlayerHealthEvent(health);
    }

    public void Die()
    {
        // Destroy Attacks and Dodges on death (Mostly for enemies and not player)
        //Debug.Log("Player Has Died");

        if (isAttacking)
        {
            OnAttackStateCancel(attackState, false);
        }
        if (isDodging)
        {
            OnDodgeCancel(false);
        }

        CancelPathfinding();

        charAgent.Teleport(Vector3.up);
        charAgent.SetVelocity(Vector3.zero);
        charAgent.SetRotation(Quaternion.Inverse(MovementController.MovementAxis) * Vector3.right);

        Heal(MaxHealth);

        isInvincible = true;

        StartCoroutine(InvincibilityTimer());

        //EnemySpawning.Instance.KillAllEnemies();
        //EnemySpawning.Instance.ResetSpawner();

        AudioManager.Instance.PlaySound("player_die1");
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

    public virtual void SwitchActiveAttack(Attack activeAttack)
    {
        if (CanSwitchActiveAttack(activeAttack))
        {
            //Debug.Log($"Switching from {this.activeAttack.AttackId} to {activeAttack.AttackId}");

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

    public void TransferToAttackState(AttackState attackState)
    {
        this.attackState = attackState;
    }

    public void InitiateAttackState(AttackState attackState)
    {
        if (CanInitiateAttackState(attackState, activeAttack.AttackId))
        {
            activeAttack.TransferToAttackState(attackState);
        }
    }

    public bool CanInitiateAttackState(AttackState attackState, string attackId)
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

    public void OnAttackStateStart(AttackState attackState)
    {
        // Can be Simplified

        if (attackState != AttackState.Idle)
        {
            isAttacking = true;
        }

        // Actions to do when the state has first started
        if (attackState == AttackState.Aiming || attackState == AttackState.Attacking)
        {
            charAgent.SetAllowMovementInput(false);
        }

        if (attackState == AttackState.ChargingUp)
        {
            charAgent.SetAllowMovementInput(false);
            charAgent.SetAllowRotationInput(false);
            //playerAnimation.Swing();
        }
        else if (attackState == AttackState.Attacking)
        {
            charAgent.SetAllowMovementInput(false);
            charAgent.SetAllowRotationInput(false);
            StartCoroutine(CancelAttack());
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
        StopCoroutine(CancelAttack());
        charAgent.SetAllowMovementInput(true);
        charAgent.SetAllowRotationInput(true);
    }

    public void OnAttackStateCancel(AttackState attackState, bool otherHasCancelled)
    {
        Debug.Log("Attack Cancelled"); // There is a bug that is cancelling the players attacks when there are many enemies or the player dies a lot

        // It is being called when the attack is idle constantly

        // Actions to do when the state is cancelled
        if (!otherHasCancelled)
        {
            if (attackState ==  AttackState.Idle)
            {
                Debug.Log($"Canceling Attack on Idle,\nIsAttacking: {isAttacking}\nActive Attack: {activeAttack.AttackId}\nActive Attack State: {activeAttack.AttackState}");
                return;
            }

            activeAttack.OnAttackStateCancel(attackState, true);
        }

        this.attackState = AttackState.Idle;
        charAgent.SetAllowMovementInput(true);
        charAgent.SetAllowRotationInput(true);
        isAttacking = false;
    }

    public void InitiateDodge()
    {
        if (CanInitiateDodge())
        {
            Vector3 dodgeDirection = GetRotationInput().normalized;

            if (dodgeDirection == Vector3.zero)
            {
                Quaternion qInverse = Quaternion.Inverse(MovementController.MovementAxis);
                dodgeDirection = qInverse * transform.forward;
            }

            dodge.InitiateDodge(dodgeDirection, GetRotationInput().normalized);
        } 
    }

    public bool CanInitiateDodge()
    {
        // Specific requirements to the class rather than the dodge
        if (dodge == null)
        {
            Debug.LogWarning($"{name} Does Not Have An Dodge");
            return false;
        }

        return (!IsDodging && attackState == AttackState.Idle && !isPathfinding);
    }

    public void OnDodgeStart()
    {
        // Actions to do when the dodge has first started
        //playerAnimation.Dodge();

        isDodging = true;
        charAgent.SetAllowMovementInput(false);
        charAgent.SetAllowRotationInput(false);
    }

    public void OnDodge()
    {
        // Actions to do while the dodge is happening
    }

    public void OnDodgeEnd()
    {
        // Actions to do when the dodge has ended
        isDodging = false;
        charAgent.SetAllowMovementInput(true);
        charAgent.SetAllowRotationInput(true);
    }

    public void OnDodgeCancel(bool otherHasCancelled)
    {
        // Actions to do when the state is cancelled
        if (!otherHasCancelled)
        {
            dodge.OnDodgeCancel(true);
        }

        isDodging = false;
        charAgent.SetAllowMovementInput(true);
        charAgent.SetAllowRotationInput(true);
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

    public IEnumerator CancelAttack()
    {
        // For Debugging Purposes
        yield return new WaitForSeconds(99f);
        OnAttackStateCancel(attackState, false);
    }

    public IEnumerator InvincibilityTimer()
    {
        yield return new WaitForSeconds(0.5f);
        isInvincible = false;
    }
}