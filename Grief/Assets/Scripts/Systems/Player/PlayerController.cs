using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Singleton<PlayerController>, IHealth, IAttack, IDodge, IAim, IPathfind
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
    private bool isAttacking;

    [SerializeField] private Dodge dodgeTemplate;
    private bool isDodging;

    private Vector3 aimDirection;
    [Space(10)]
    [Header("Aiming")]
    [SerializeField] private float rotationSpeed;
    private bool isAiming;

    private Vector3 pathfindPosition;

    // Interface Implementation Fields
    // ---------------------------------------------------------------------------------------------------------
    public float MaxHealth { get { return maxHealth; } }
    public float Health { get { return health; } }
    public Attack Attack { get { return attackTemplate; } }
    public bool IsAttacking { get { return isAttacking; } }
    public Dodge Dodge { get { return dodgeTemplate; } }
    public bool IsDodging { get { return isDodging; } }
    public Vector3 AimDirection { get { return aimDirection; } }
    public float RotationSpeed { get { return rotationSpeed; } }
    public bool IsAiming { get { return isAiming; } }
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
            InitiateAttack();
        }
    }

    private void OnActionCancelled(InputAction.CallbackContext obj)
    {
        //Debug.Log($"Action Button Cancelled {obj.action.activeControl.name}");
        if (obj.action.activeControl.name == "buttonWest" && isAiming)
        {
            attack.SendCommand(AttackCommand.AttackButtonReleased, transform);
            isAiming = false;
        }
    }

    private void Start()
    {
        CameraManager.Instance.TransferCameraTo(transform);
    }

    private void Update()
    {
        if (attack.AttackState == AttackState.Aiming)
        {
            if (!isAiming)
            {
                isAiming = true;
            }

            Aim();
            attack.OnAim(transform);
        }
        else
        {
            if (isAiming)
            {
                isAiming = false;
            }

            playerMovement.UpdatePlayerRotation(playerTransform);
        }

        if (attack.AttackState == AttackState.Attacking)
        {
            if (!isAttacking)
            {
                OnAttackStart();
            }

            attack.OnAttack(transform);
        }
        else
        {
            if (isAttacking)
            {
                OnAttackCancel();
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isDodging && !isAiming && !isAttacking) 
        {
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

    public void InitiateAttack()
    {
        if (!CanAttack())
        {
            return;
        }

        attack.SendCommand(AttackCommand.AttackButtonPressed, transform);
    }

    public bool CanAttack()
    {
        return !isAttacking && !isAiming;
    }

    public void OnAttackStart()
    {
        StartCoroutine(AttackDelay());
        isAttacking = true;
        //Debug.Log("Player Attack Started");
    }

    public void OnAttackCancel()
    {
        isAiming = false;
        isAttacking = false;
    }

    public void OnAttackEnd()
    {
        attack.SendCommand(AttackCommand.AttackAnimationFinished, transform);
        isAttacking = false;
        //Debug.Log("Player Attack Ended");
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

    public bool CanAim()
    {
        return !isDodging && !isAttacking;
    }

    public bool Pathfind()
    {
        return false;
    }

    public IEnumerator AttackDelay()
    {
        yield return new WaitForSeconds(0.5f);
        OnAttackEnd();
    }
}