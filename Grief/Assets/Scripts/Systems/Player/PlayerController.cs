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

    // Interface Variables
    // ---------------------------------------------------------------------------------------------------------
    [Space(10)]
    [Header("Health")]
    [SerializeField] private float maxHealth;
    private float health;

    [Space(10)]
    [Header("Attack and Dodging")]
    [SerializeField] private Attack attack;
    private bool isAttacking;

    [SerializeField] private Dodge dodge;
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
    public Attack Attack { get { return attack; } }
    public bool IsAttacking { get { return isAttacking; } }
    public Dodge Dodge { get { return dodge; } }
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

        playerInputControls.Player.Action.performed += DoAction;
        playerInputControls.Player.Action.Enable();
    }

    private void DoAction(InputAction.CallbackContext obj)
    {
        //Debug.Log($"Action Button Pressed {obj.action.activeControl.name}");
        if (obj.action.activeControl.name == "buttonEast")
        {
            InitiateDodge();
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
        if (!isDodging) {

            playerMovement.HandleMovement(characterController, leftJoystick.ReadValue<Vector2>(), maxSpeed, totalAccelerationTime, totalDeaccelerationTime, inputSmoothMultiplier);
        }

        playerMovement.HandleGravity(characterController);
        playerMovement.UpdatePlayerRotation(playerTransform);
    }

    private void LateUpdate()
    {
        playerLook.PanTowards(rightJoystick.ReadValue<Vector2>(), panStrength, panTimeMultiplier);
    }

    private void OnDisable()
    {
        leftJoystick.Disable();
        rightJoystick.Disable();

        playerInputControls.Player.Action.performed -= DoAction;
        playerInputControls.Player.Action.Disable();
    }

    // Interface Implementation Methods
    // ---------------------------------------------------------------------------------------------------------
    public void Damage(float damage)
    {

    }

    public void Heal(float healing)
    {

    }

    public void Die()
    {

    }

    public void InitiateAttack()
    {

    }

    public bool CanAttack()
    {
        return false;
    }

    public void OnAttackStart()
    {

    }

    public void OnAttackEnd()
    {

    }

    public void InitiateDodge()
    {
        if (!CanDodge())
        {
            return;
        }

        isDodging = true;

        Vector3 dodgeDirection = transform.forward;

        if (leftJoystick.ReadValue<Vector2>().magnitude > 0)
        {
            dodgeDirection = PlayerMovement.movementAxis * leftJoystick.ReadValue<Vector2>().normalized;
        }

        playerMovement.CurrentDirection = dodgeDirection;
        playerMovement.CurrentSpeed = dodge.DodgeSpeed;
        playerMovement.ResetInput();
        playerMovement.UpdatePlayerRotation(playerTransform);

        dodge.InitiateDodge(this, characterController, playerMovement.CurrentDirection);
    }

    public bool CanDodge()
    {
        return !isDodging;
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
        return false;
    }

    public bool CanAim()
    {
        return false;
    }

    public bool Pathfind()
    {
        return false;
    }
}