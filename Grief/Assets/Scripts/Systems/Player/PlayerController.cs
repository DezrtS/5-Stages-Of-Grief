using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Singleton<PlayerController>, IHealth, IAttack, IDodge, IPathfind
{
    private Transform playerTransform;
    private CharacterController characterController;
    //[SerializeField] private Transform modelTransform;

    private PlayerMovement playerMovement;

    private PlayerLook playerLook;

    private PlayerInputControls playerInputControls;
    private InputAction leftJoystick;
    private InputAction rightJoystick;

    [Header("Movement")]
    [SerializeField] private float maxSpeed = 15;
    [Range(0, 100)]
    [SerializeField] private float totalAccelerationTime = 0.5f;
    [Range(0, 100)]
    [SerializeField] private float totalDeaccelerationTime = 0.5f;
    [SerializeField] private float inputSmoothMultiplier = 6;

    [Space(10)]
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
        playerMovement = transform.AddComponent<PlayerMovement>();
        playerLook = transform.AddComponent<PlayerLook>();

        dodge = dodgeTemplate.Clone();
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
            InitiateState(AttackState.Aiming);
        }
    }

    private void OnActionCancelled(InputAction.CallbackContext obj)
    {
        //Debug.Log($"Action Button Cancelled {obj.action.activeControl.name}");
        if (obj.action.activeControl.name == "buttonWest" && attackState == AttackState.Aiming)
        {
            InitiateState(AttackState.Attacking);
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
        if (!isDodging && attackState != AttackState.Aiming && attackState != AttackState.Attacking) 
        {
            playerMovement.UpdatePlayerRotation(playerTransform);
            playerMovement.HandleMovement2(characterController, leftJoystick.ReadValue<Vector2>(), maxSpeed, totalAccelerationTime, totalDeaccelerationTime, inputSmoothMultiplier);
        } 
        else if (!isDodging)
        {
            playerMovement.HandleMovement2(characterController, Vector2.zero, maxSpeed, totalAccelerationTime, totalDeaccelerationTime, inputSmoothMultiplier);
        }

        //playerMovement.HandleGravity(characterController);
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

    public bool TransferToState(AttackState attackState)
    {
        if (CanInitiateState(attackState))
        {
            this.attackState = attackState;
            return true;
        }

        return false;
    }

    public void InitiateState(AttackState attackState)
    {
        if (CanInitiateState(attackState))
        {
            attack.TransferToState(attackState, this, transform);
        } 
        else
        {
            Debug.Log($"Cannot Enter State {attackState}");
        }
    }

    public bool CanInitiateState(AttackState attackState)
    {
        // Specific requirements to the class rather than the attack
        if (this.attackState == attackState)
        {
            return false;
        }

        return true;
    }

    public void OnStateStart(AttackState attackState)
    {
        // Actions to do when the state has first started
        if (attackState == AttackState.Attacking)
        {
            StartCoroutine(EndAttack());
        }
    }

    public void OnState(AttackState attackState)
    {
        // Actions to do while the state is happening
        if (attackState == AttackState.Aiming)
        {
            Aim();
        }
    }

    public void OnStateEnd(AttackState attackState)
    {
        // Actions to do when the state has ended
    }

    public void OnStateCancel(AttackState attackState)
    {
        // Actions to do when the state is cancelled
        attackState = AttackState.Idle;
    }

    public void InitiateDodge()
    {
        if (!CanDodge())
        {
            return;
        }

        isDodging = true;

        Debug.Log("Player Preparing For Dodge");
        playerMovement.PrepareForDodge2(playerTransform, leftJoystick.ReadValue<Vector2>(), dodge);

        dodge.InitiateDodge(this, characterController, playerMovement.CurrentDirection);
    }
     
    public bool CanDodge()
    {
        if (!isDodging && dodge != null)
        {
            if (dodge.CanDodge())
            {
                return true;
            }
        }

        return false;
    }

    public void OnDodgeStart()
    {
        Debug.Log("Player Dodge Started");
    }

    public void OnDodgeEnd()
    {
        isDodging = false;
        Debug.Log("Player Dodge Ended");
    }

    public bool Aim()
    {
        Vector3 currentDirection = playerTransform.forward;
        Vector3 targetDirection = PlayerMovement.movementAxis * leftJoystick.ReadValue<Vector2>().normalized;

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

    public IEnumerator EndAttack()
    {
        yield return new WaitForSeconds(1f);
        InitiateState(AttackState.Idle);
    }
}