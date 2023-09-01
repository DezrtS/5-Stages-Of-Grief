using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerController : Singleton<PlayerController>, IHealth, IMove, IAttack, IDodge, IPathfind
{
    private Transform playerTransform;
    private CharacterController characterController;
    private NavMeshAgent agent;
    [SerializeField] private GameObject pathfindObject;
    //[SerializeField] private Transform modelTransform;

    private PlayerLook playerLook;

    private PlayerInputControls playerInputControls;
    private InputAction leftJoystick;
    private InputAction rightJoystick;

    private RigidTransform rigidTrans;

    [Header("Camera Panning")]
    [SerializeField] private float panStrength = 5;
    [Range(0.1f, 100)]
    [SerializeField] private float panTimeMultiplier = 10;

    private Attack attack;
    private Dodge dodge;

    // Interface Variables
    // ---------------------------------------------------------------------------------------------------------
    [Space(10)]
    [Header("Health")]
    private readonly HealthType healthType = HealthType.Player;
    [SerializeField] private float maxHealth;
    private float health;
    private bool isInvincible;

    [Space(10)]
    [Header("Attack and Dodging")]
    [SerializeField] private List<HealthType> damageTypes;
    [SerializeField] private Attack attackTemplate;
    private AttackState attackState;

    [SerializeField] private Dodge dodgeTemplate;
    private bool isDodging;

    [Space(10)]
    [Header("Aiming")]
    [SerializeField] private float rotationSpeed;

    private bool isPathfinding;
    private Vector3 pathfindDestination;

    // Interface Implementation Fields
    // ---------------------------------------------------------------------------------------------------------
    public HealthType HealthType { get { return healthType; } }
    public float MaxHealth { get { return maxHealth; } }
    public float Health { get { return health; } }
    public bool IsInvincible { get { return isInvincible; } }
    public Attack Attack { get { return attackTemplate; } }
    public AttackState AttackState { get { return attackState; } }
    public List<HealthType> DamageTypes { get { return damageTypes; } }
    public Dodge Dodge { get { return dodgeTemplate; } }
    public bool IsDodging { get { return isDodging; } }
    public float RotationSpeed { get { return rotationSpeed; } }
    public bool IsPathfinding { get { return isPathfinding; } }
    public Vector3 PathfindDestination { get { return pathfindDestination; } set { pathfindDestination = value; } }
    // ---------------------------------------------------------------------------------------------------------

    protected override void Awake()
    {
        base.Awake();

        playerTransform = transform;
        characterController = GetComponent<CharacterController>();
        agent = GetComponent<NavMeshAgent>();
        rigidTrans = GetComponent<RigidTransform>();
        playerLook = transform.AddComponent<PlayerLook>();

        dodge = dodgeTemplate.Clone(dodge, this, rigidTrans);
        attack = attackTemplate.Clone(attack, this, transform);

        health = maxHealth;
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
        //Debug.Log($"Action Button Pressed {obj.action.activeControl.name}");
        if (obj.action.activeControl.name == "buttonEast")
        {
            InitiateDodge();
        } 
        else if (obj.action.activeControl.name == "buttonWest")
        {
            InitiateAttackState(AttackState.Aiming);
        } 
        else if (obj.action.activeControl.name == "buttonNorth")
        {
            pathfindDestination = (new Vector3(pathfindObject.transform.position.x, transform.position.y, pathfindObject.transform.position.z));
            InitiatePathfinding();
        }
    }

    private void OnActionCancelled(InputAction.CallbackContext obj)
    {
        //Debug.Log($"Action Button Cancelled {obj.action.activeControl.name}");
        if (obj.action.activeControl.name == "buttonWest" && attackState == AttackState.Aiming)
        {
            InitiateAttackState(AttackState.Attacking);
        }
    }

    private void Start()
    {
        agent.enabled = false;
        CameraManager.Instance.TransferCameraTo(transform);
    }

    private void LateUpdate()
    {
        playerLook.PanTowards(rightJoystick.ReadValue<Vector2>(), panStrength, panTimeMultiplier);
    }

    private void OnDisable()
    {
        leftJoystick.Disable();
        rightJoystick.Disable();

        playerInputControls.Player.Action.performed -= OnActionPerformed;
        playerInputControls.Player.Action.canceled -= OnActionCancelled;
        playerInputControls.Player.Action.Disable();
    }

    // Interface Implementation Methods
    // ---------------------------------------------------------------------------------------------------------
    public void Damage(float damage)
    {
        if (isInvincible)
        {
            return;
        }

        health = Mathf.Max(health - damage, 0);

        if (health == 0)
        {
            Die();
        }
    }

    public void Heal(float healing)
    {
        health = Mathf.Min(health + healing, maxHealth);
    }

    public void Die()
    {
        // Destroy Attacks and Dodges on death (Mostly for enemies and not player)
        Debug.Log("Player Has Died");
    }

    public Vector3 GetMovementInput()
    {
        if (isPathfinding)
        {

        }
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

        if (this.attackState == attackState)
        {
            return false;
        } 
        else if (this.attackState == AttackState.Attacking && attackState == AttackState.Aiming)
        {
            return false;
        }

        return (!isDodging && !isPathfinding);
    }

    public void OnAttackStateStart(AttackState attackState)
    {
        // Actions to do when the state has first started
        if (attackState == AttackState.Aiming || attackState == AttackState.Attacking)
        {
            rigidTrans.SetCanMove(false);
        }

        if (attackState == AttackState.Attacking)
        {
            rigidTrans.SetCanRotate(false);
            StartCoroutine(CancelAttack());
        }
        else if (attackState == AttackState.Idle)
        {
            rigidTrans.SetCanMove(true);
            rigidTrans.SetCanRotate(true);
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
        rigidTrans.SetCanMove(true);
        rigidTrans.SetCanRotate(true);
    }

    public void OnAttackStateCancel(AttackState attackState, bool otherHasCancelled)
    {
        // Actions to do when the state is cancelled
        if (!otherHasCancelled)
        {
            attack.OnAttackStateCancel(attackState, true);
        }

        this.attackState = AttackState.Idle;
        rigidTrans.SetCanMove(true);
        rigidTrans.SetCanRotate(true);
    }

    public void InitiateDodge()
    {
        if (CanInitiateDodge())
        {
            Vector3 dodgeDirection = GetRotationInput().normalized;

            if (dodgeDirection == Vector3.zero)
            {
                Quaternion qInverse = Quaternion.Inverse(RigidTransform.MovementAxis);
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
        rigidTrans.SetCanMove(false);
        rigidTrans.SetCanRotate(false);
    }

    public void OnDodge()
    {
        // Actions to do while the dodge is happening
    }

    public void OnDodgeEnd()
    {
        // Actions to do when the dodge has ended
        isDodging = false;
        rigidTrans.SetCanMove(true);
        rigidTrans.SetCanRotate(true);
    }

    public void OnDodgeCancel(bool otherHasCancelled)
    {
        // Actions to do when the state is cancelled
        if (!otherHasCancelled)
        {
            dodge.OnDodgeCancel(true);
        }

        isDodging = false;
        rigidTrans.SetCanMove(true);
        rigidTrans.SetCanRotate(true);
    }

    public bool Aim()
    {
        Vector3 currentDirection = playerTransform.forward;
        Vector3 targetDirection = RigidTransform.MovementAxis * leftJoystick.ReadValue<Vector2>().normalized;

        if (targetDirection.magnitude > 0)
        {
            rigidTrans.ForceRotation(GetRotationInput());

            return currentDirection == targetDirection;
        }
        
        return true;
    }

    public void TransferToPathfindingState(bool isPathfinding)
    {
        this.isPathfinding = isPathfinding;
    }

    public void InitiatePathfinding()
    {
        if (CanInitiatePathfinding())
        {
            rigidTrans.InitiatePathfinding(pathfindDestination, transform);
        }
    }

    public bool CanInitiatePathfinding()
    {
        return (!isDodging && attackState == AttackState.Idle);
    }

    public void CancelPathfinding()
    {
        rigidTrans.StopPathfinding();
    }
    // ---------------------------------------------------------------------------------------------------------

    public IEnumerator CancelAttack()
    {
        // For Debugging Purposes
        yield return new WaitForSeconds(99f);
        OnAttackStateCancel(attackState, false);
    }
}