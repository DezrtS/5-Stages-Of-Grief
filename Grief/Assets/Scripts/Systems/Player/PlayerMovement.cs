using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static Quaternion movementAxis = Quaternion.Euler(90, 0, -45);

    private float currentSpeed = 0;
    private Vector3 currentDirection = Vector3.right;
    private Vector2 previousInput = Vector2.zero;

    public float CurrentSpeed { get { return currentSpeed; } set { currentSpeed = value; } }
    public Vector3 CurrentDirection { get { return currentDirection; } set { currentDirection = value; } }

    private float gravity = 3;

    public void HandleMovement(CharacterController characterController, Vector2 input, float maxSpeed, float totalAccelerationTime, float totalDeaccelerationTime, float inputSmoothMultiplier)
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

    public void HandleGravity(CharacterController characterController)
    {
        characterController.Move(Vector2.down * gravity * Time.deltaTime);
    }

    public void UpdatePlayerRotation(Transform transform)
    {
        if (currentDirection != Vector3.zero)
        {
            transform.forward = currentDirection;
        }
    }

    public float GetAcceleration(float maxSpeed, float timeToReachFullSpeed)
    {
        return (maxSpeed) / timeToReachFullSpeed;
    }

    public void ResetInput()
    {
        previousInput = Vector2.zero;
    }

    public bool isGrounded()
    {
        return true;
    }
}
