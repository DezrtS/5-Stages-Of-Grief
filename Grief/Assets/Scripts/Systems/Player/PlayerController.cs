using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerController : Singleton<PlayerController>, IHealth, IMove, IAttack, IDodge, IPathfind
{
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
    [SerializeField] private Attack attackTemplate;
    private Attack attack;
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
    public Attack Attack { get { return attackTemplate; } }
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
        attack = attackTemplate.Clone(attack, this, transform);

        health = maxHealth;
    }

    private void Start()
    {
        CameraManager.Instance.TransferCameraTo(transform);
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
    }

    private void OnActionPerformed(InputAction.CallbackContext obj)
    {
        string buttonName = obj.action.activeControl.name;

        switch (buttonName)
        {
            case "buttonNorth":
                pathfindDestination = (new Vector3(pathfindObject.transform.position.x, transform.position.y, pathfindObject.transform.position.z));
                InitiatePathfinding();
                return;
            case "buttonEast":

                return;
            case "buttonSouth":
                InitiateDodge();
                return;
            case "buttonWest":
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

    private void OnDisable()
    {
        leftJoystick.Disable();
        rightJoystick.Disable();

        playerInputControls.Player.Action.performed -= OnActionPerformed;
        playerInputControls.Player.Action.canceled -= OnActionCancelled;
        playerInputControls.Player.Action.Disable();
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

        CharacterController characterController = GetComponent<CharacterController>();

        characterController.enabled = false;
        transform.position = Vector3.up;
        transform.rotation = Quaternion.identity;
        characterController.enabled = true;

        charAgent.SetVelocity(Vector3.zero);
        charAgent.SetRotation(Quaternion.Inverse(MovementController.MovementAxis) * transform.forward);

        Heal(MaxHealth);

        isInvincible = true;

        StartCoroutine(InvincibilityTimer());

        //attack.DestroyClone();
        //dodge.DestroyClone();

        //SceneManager.Instance.ResetScene();
    }

    public Vector3 GetMovementInput()
    {
        return leftJoystick.ReadValue<Vector2>();
    }

    public Vector3 GetRotationInput()
    {
        return GetMovementInput();
    }

    public void TransferToAttackState(AttackState attackState)
    {
        this.attackState = attackState;
    }

    public void InitiateAttackState(AttackState attackState)
    {
        if (CanInitiateAttackState(attackState, attack.AttackId))
        {
            if (attackState != AttackState.Idle)
            {
                isAttacking = true;
            }
            attack.TransferToAttackState(attackState);
        }
    }

    public bool CanInitiateAttackState(AttackState attackState, string attackId)
    {
        // Specific requirements to the class rather than the attack
        if (attack == null)
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

        // Actions to do when the state has first started
        if (attackState == AttackState.Aiming || attackState == AttackState.Attacking)
        {
            charAgent.SetAllowMovementInput(false);
        }

        if (attackState == AttackState.ChargingUp)
        {
            charAgent.SetAllowMovementInput(false);
            charAgent.SetAllowRotationInput(false);
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
        // Actions to do when the state is cancelled
        if (!otherHasCancelled)
        {
            attack.OnAttackStateCancel(attackState, true);
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