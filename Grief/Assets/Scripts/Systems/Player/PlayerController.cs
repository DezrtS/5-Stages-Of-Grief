using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Singleton<PlayerController>, IHealth, IMove, IAttack, IDodge, IPathfind
{
    private Transform playerTransform;
    private CharacterController characterController;
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
    [SerializeField] private float maxHealth;
    private float health;

    [Space(10)]
    [Header("Attack and Dodging")]
    [SerializeField] private Attack attackTemplate;
    private AttackState attackState;

    [SerializeField] private Dodge dodgeTemplate;
    private bool isDodging;

    private Vector3 aimDirection;
    [Space(10)]
    [Header("Aiming")]
    [SerializeField] private float rotationSpeed;

    private Vector3 pathfindPosition;

    // Interface Implementation Fields
    // ---------------------------------------------------------------------------------------------------------
    public float MaxHealth { get { return maxHealth; } }
    public float Health { get { return health; } }
    public Attack Attack { get { return attackTemplate; } }
    public AttackState AttackState { get { return attackState; } }
    public Dodge Dodge { get { return dodgeTemplate; } }
    public bool IsDodging { get { return isDodging; } }
    public Vector3 AimDirection { get { return aimDirection; } }
    public float RotationSpeed { get { return rotationSpeed; } }
    public Vector3 PathfindPosition { get { return pathfindPosition; } set { pathfindPosition = value; } }

    protected override void Awake()
    {
        base.Awake();

        playerTransform = transform;
        characterController = GetComponent<CharacterController>();
        rigidTrans = GetComponent<RigidTransform>();
        playerLook = transform.AddComponent<PlayerLook>();

        dodge = dodgeTemplate.Clone(dodge);
        attack = attackTemplate.Clone(attack);
    }

    private void OnEnable()
    {
        if (playerInputControls == null)
        {
            playerInputControls = new PlayerInputControls();
        }

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
        CameraManager.Instance.TransferCameraTo(transform);
    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {

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
        Debug.Log("Player Has Died");
    }

    public Vector3 GetMovementInput()
    {
        return leftJoystick.ReadValue<Vector2>();
    }

    public Vector3 GetRotationInput()
    {
        return GetMovementInput();
    }

    public bool TransferToAttackState(AttackState attackState)
    {
        if (CanInitiateAttackState(attackState))
        {
            this.attackState = attackState;

            rigidTrans.SetCanMove(attackState == AttackState.Idle);

            return true;
        }

        return false;
    }

    public void InitiateAttackState(AttackState attackState)
    {
        if (CanInitiateAttackState(attackState))
        {
            attack.TransferToAttackState(attackState, this, transform);
        } 
        else
        {
            Debug.Log($"Cannot Enter State {attackState}");
        }
    }

    public bool CanInitiateAttackState(AttackState attackState)
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

        return !isDodging;
    }

    public void OnAttackStateStart(AttackState attackState)
    {
        // Actions to do when the state has first started
        if (attackState == AttackState.Attacking)
        {
            StartCoroutine(CancelAttack());
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
    }

    public void OnAttackStateCancel(AttackState attackState, bool otherHasCancelled)
    {
        // Actions to do when the state is cancelled
        if (!otherHasCancelled)
        {
            attack.OnAttackStateCancel(attackState, this, true);
        }

        this.attackState = AttackState.Idle;
        rigidTrans.SetCanMove(true);
        rigidTrans.SetCanRotate(true);
    }

    public void InitiateDodge()
    {
        if (CanInitiateDodge())
        {
            Vector3 dodgeDirection = RigidTransform.MovementAxis * GetMovementInput().normalized;

            if (dodgeDirection == Vector3.zero)
            {
                dodgeDirection = transform.forward;
            }

            dodge.InitiateDodge(dodgeDirection, this, rigidTrans);
        } 
        else
        {
            Debug.Log($"Cannot Dodge");
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

        return !IsDodging && attackState == AttackState.Idle;
    }

    public void OnDodgeStart()
    {
        // Actions to do when the dodge has first started

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
            dodge.OnDodgeCancel(this, true);
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
            playerTransform.forward = targetDirection;
            
            return currentDirection == targetDirection;
        }
        
        return true;
    }

    public bool Pathfind()
    {
        return false;
    }

    // ---------------------------------------------------------------------------------------------------------

    public IEnumerator CancelAttack()
    {
        // For Debugging Purposes
        yield return new WaitForSeconds(1f);
        OnAttackStateCancel(attackState, false);
    }
}