using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static Quaternion movementAxis = Quaternion.Euler(90, 0, -45);

    private float currentSpeed = 0;
    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 currentDirection = Vector3.right;
    private Vector2 previousInput = Vector2.zero;

    public float CurrentSpeed { get { return currentSpeed; } set { currentSpeed = value; } }
    public Vector3 CurrentDirection { get { return currentDirection; } set { currentDirection = value; } }

    private float gravityVelocity = 0;
    private float gravity = 19.6f;

    public void HandleMovement2(CharacterController characterController, Vector2 input, float maxSpeed, float totalAccelerationTime, float totalDeaccelerationTime, float inputSmoothMultiplier)
    {
        Vector2 smoothedInput = Vector2.Lerp(previousInput, input, Time.deltaTime * inputSmoothMultiplier);
        previousInput = smoothedInput;

        Vector3 targetDirection = movementAxis * smoothedInput.normalized;
        Vector3 targetVelocity = targetDirection * maxSpeed * smoothedInput.magnitude;
        float targetSpeed = targetVelocity.magnitude;

        Vector3 velocityDifference = targetVelocity - currentVelocity;
        Vector3 differenceDirection = velocityDifference.normalized;

        if (currentSpeed < targetSpeed)
        {
            float accelerationIncrement = GetAcceleration(maxSpeed, totalAccelerationTime) * Time.deltaTime;

            if (velocityDifference.magnitude < accelerationIncrement)
            {
                currentVelocity = targetVelocity;
            }
            else
            {
                currentVelocity = currentVelocity + differenceDirection * accelerationIncrement;
            }
            currentSpeed = currentVelocity.magnitude;
            
        }
        else if (currentSpeed > targetSpeed)
        {
            float deaccelerationIncrement = GetAcceleration(maxSpeed, totalDeaccelerationTime) * Time.deltaTime;

            if (velocityDifference.magnitude < deaccelerationIncrement)
            {
                currentVelocity = targetVelocity;
            }
            else
            {
                currentVelocity = currentVelocity + differenceDirection * deaccelerationIncrement;
            }

            currentSpeed = currentVelocity.magnitude;
        }

        characterController.Move(currentVelocity * Time.deltaTime);

        if (targetDirection != Vector3.zero)
        {
            currentDirection = targetDirection;
        }
    }

    public void HandleMovement(CharacterController characterController, Vector2 input, float maxSpeed, float totalAccelerationTime, float totalDeaccelerationTime, float inputSmoothMultiplier)
    {
        Vector2 smoothedInput = Vector2.Lerp(previousInput, input, Time.deltaTime * inputSmoothMultiplier);
        previousInput = smoothedInput;

        Vector3 targetDirection = movementAxis * smoothedInput.normalized;
        Vector3 targetVelocity = targetDirection * maxSpeed * smoothedInput.magnitude;
        float targetSpeed = targetVelocity.magnitude;

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
        if (characterController.isGrounded)
        {
            gravityVelocity = 0.5f;
        }

        gravityVelocity += gravity * Time.deltaTime;
        characterController.Move(Vector2.down * gravityVelocity * Time.deltaTime);
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

    public void PrepareForDodge(Transform transform, Vector2 input, Dodge dodge)
    {
        Vector3 dodgeDirection = transform.forward;

        if (input.magnitude > 0)
        {
            dodgeDirection = movementAxis * input.normalized;
        }

        currentDirection = dodgeDirection;
        currentSpeed = dodge.DodgeSpeed;
        previousInput = Vector2.zero;
        UpdatePlayerRotation(transform);
    }

    public void PrepareForDodge2(Transform transform, Vector2 input, Dodge dodge)
    {
        Vector3 dodgeDirection = transform.forward;

        if (input.magnitude > 0)
        {
            dodgeDirection = movementAxis * input.normalized;
        }

        currentDirection = dodgeDirection;
        currentSpeed = dodge.DodgeSpeed;
        currentVelocity = currentDirection * currentSpeed;
        previousInput = Vector2.zero;
        UpdatePlayerRotation(transform);
    }
}
