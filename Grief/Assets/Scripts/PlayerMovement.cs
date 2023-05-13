using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;
    private PlayerInputControls playerInputControls;
    private InputAction playerMovement;
    private InputAction playerLook;

    private Transform playerTransform;

    // Debug
    [SerializeField] private Transform playerTargetLook;

    [SerializeField] private Transform playerModel;

    [SerializeField] private float maxSpeed = 15;
    [SerializeField] private float totalAccelerationTime = 5f;
    [SerializeField] private float totalDeaccelerationTime = 10f;
    [SerializeField] private float inputSmoothMultiplier = 2;

    private float gravity;
    private float currentSpeed = 0;
    private Vector3 currentDirection = Vector3.right;
    private Vector2 previousInput = Vector2.zero;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInputControls = new PlayerInputControls();
        playerTransform = transform;
    }

    private void OnEnable()
    {
        playerMovement = playerInputControls.Player.Movement;
        playerMovement.Enable();

        playerLook = playerInputControls.Player.Look;
        playerLook.Enable();

        playerInputControls.Player.Action.performed += DoAction;
        playerInputControls.Player.Action.Enable();
    }

    private void DoAction(InputAction.CallbackContext obj)
    {
        Debug.Log($"Action Button Pressed {obj.action.activeControl.name}");
    }

    void Update()
    {
        Vector2 input = playerMovement.ReadValue<Vector2>();
        Vector2 smoothedInput = Vector2.Lerp(previousInput, input, Time.deltaTime * inputSmoothMultiplier);
        previousInput = smoothedInput;

        Vector3 targetDirection = (playerTransform.right * smoothedInput.x + playerTransform.forward * smoothedInput.y).normalized;
        Vector3 targetVelocity = targetDirection * maxSpeed * smoothedInput.magnitude;
        float targetSpeed = targetVelocity.magnitude;

        if (currentSpeed == 0)
        {
            currentDirection = targetDirection;
        }

        if (currentSpeed < targetSpeed)
        {
            float accelerationIncrement = GetAcceleration(maxSpeed, totalAccelerationTime) * Time.deltaTime;
            if (Mathf.Abs(currentSpeed - targetSpeed) < accelerationIncrement)
            {
                currentSpeed = targetSpeed;
            } 
            else
            {
                currentSpeed = currentSpeed + accelerationIncrement;
            }
        } 
        else if (currentSpeed > targetSpeed)
        {
            float deaccelerationIncrement = GetAcceleration(maxSpeed, totalDeaccelerationTime) * Time.deltaTime;
            if (Mathf.Abs(currentSpeed - targetSpeed) < deaccelerationIncrement)
            {
                currentSpeed = targetSpeed;
            }
            else
            {
                currentSpeed = currentSpeed - deaccelerationIncrement;
            }
        }

        //Debug.Log($"Current Speed: {currentSpeed}, Target Speed: {targetSpeed}, Current Direction: {currentDirection}, Target Direction: {targetDirection}");

        characterController.Move(currentSpeed * currentDirection * Time.deltaTime);

        if (targetDirection != Vector3.zero)
        {
            // Debug
            playerTargetLook.forward = (playerTransform.right * input.x + playerTransform.forward * input.y).normalized;

            playerModel.forward = currentDirection;
            currentDirection = targetDirection;
        }
    }

    private void OnDisable()
    {
        playerMovement.Disable();
        playerLook.Disable();
        playerInputControls.Player.Action.Disable();
    }

    public float GetAcceleration(float maxSpeed, float timeToReachFullSpeed)
    {
        return (maxSpeed) / timeToReachFullSpeed;
    }
}
