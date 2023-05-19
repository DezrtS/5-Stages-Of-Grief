using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static Quaternion movementAxis = Quaternion.Euler(90, 0, -45);

    private CharacterController characterController;

    [SerializeField] private Transform playerModel;

    [SerializeField] private float maxSpeed = 15;
    [SerializeField] private float totalAccelerationTime = 0.5f;
    [SerializeField] private float totalDeaccelerationTime = 0.5f;
    [SerializeField] private float inputSmoothMultiplier = 6;

    [SerializeField] private float dodgeSpeed = 15;
    [SerializeField] private float dodgeTime = 1;

    private float gravity = 3;

    private float currentSpeed = 0;
    private Vector3 currentDirection = Vector3.right;
    private Vector2 previousInput = Vector2.zero;

    private bool isDodging = false;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void MovePlayer(Transform player, Vector2 input)
    {
        if (isDodging)
        {
            characterController.Move(dodgeSpeed * currentDirection * Time.deltaTime);
        }
        else
        {
            Vector2 smoothedInput = Vector2.Lerp(previousInput, input, Time.deltaTime * inputSmoothMultiplier);
            previousInput = smoothedInput;

            Vector3 targetDirection = movementAxis * smoothedInput.normalized;
            Vector3 targetVelocity = targetDirection * maxSpeed * smoothedInput.magnitude;
            float targetSpeed = targetVelocity.magnitude;

            if (currentSpeed == 0)
            {
                //currentDirection = targetDirection;
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

            characterController.Move(currentSpeed * currentDirection * Time.deltaTime);

            if (targetDirection != Vector3.zero)
            {
                currentDirection = targetDirection;
            }
        }
    }

    public void UpdatePlayerRotation(Transform player)
    {
        if (currentDirection != Vector3.zero)
        {
            player.forward = currentDirection;
        }
    }

    public void Dodge(Transform player, Vector2 input)
    {
        if (isDodging)
        {
            return;
        }

        isDodging = true;
        
        if (input.magnitude == 0)
        {
            currentDirection = player.forward;
        } 
        else
        {
            currentDirection = movementAxis * input.normalized;
        }

        StartCoroutine(DodgeTimer());
    }

    public void ApplyGravity(Transform player)
    {
        characterController.Move(Vector2.down * gravity * Time.deltaTime);
    }

    public float GetAcceleration(float maxSpeed, float timeToReachFullSpeed)
    {
        return (maxSpeed) / timeToReachFullSpeed;
    }

    private IEnumerator DodgeTimer()
    {
        yield return new WaitForSeconds(dodgeTime);
        isDodging = false;
    }
}
