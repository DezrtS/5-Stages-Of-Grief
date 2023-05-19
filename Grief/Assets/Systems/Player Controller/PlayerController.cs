using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Singleton<PlayerController>
{
    private PlayerMovement playerMovement;
    private PlayerLook playerLook;

    private Transform playerTransform;
    //[SerializeField] private Transform modelTransform;

    private PlayerInputControls playerInputControls;
    private InputAction leftJoystick;
    private InputAction rightJoystick;

    public override void Awake()
    {
        base.Awake();

        playerTransform = transform;

        bool hasPlayerMovement = TryGetComponent<PlayerMovement>(out playerMovement);
        bool hasPlayerLook = TryGetComponent<PlayerLook>(out playerLook);

        if (!hasPlayerMovement)
        {
            playerMovement = transform.AddComponent<PlayerMovement>();
        }

        if (!hasPlayerLook)
        {
            playerLook = transform.AddComponent<PlayerLook>();
        }
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
        Debug.Log($"Action Button Pressed {obj.action.activeControl.name}");
        if (obj.action.activeControl.name == "buttonEast")
        {
            playerMovement.Dodge(playerTransform, leftJoystick.ReadValue<Vector2>());
        }
    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {
        playerMovement.MovePlayer(playerTransform, leftJoystick.ReadValue<Vector2>());
        playerMovement.UpdatePlayerRotation(playerTransform);
        playerMovement.ApplyGravity(playerTransform);
    }

    private void LateUpdate()
    {
        playerLook.LookTowards(playerTransform, rightJoystick.ReadValue<Vector2>());
    }

    private void OnDisable()
    {
        leftJoystick.Disable();
        rightJoystick.Disable();
        playerInputControls.Player.Action.Disable();
    }
}
