using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;

    private float maxSpeed = 15;
    private float accelerateTime = 1f;
    private float deccelerateTime = 5;

    private float startMovementTime = -1;

    bool accelerating = false;
    bool isAtFullSpeed = false;
    bool deaccelerating = false;

    private float gravity;
    private Vector3 velocity;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }


    void Update()
    {
        float xInput = Input.GetAxisRaw("Horizontal");
        float zInput = Input.GetAxisRaw("Vertical");



        velocity = (transform.right * xInput + transform.forward * zInput).normalized * maxSpeed * Time.deltaTime;

        if (velocity.magnitude > 0 && !accelerating && !isAtFullSpeed)
        {
            startMovementTime = Time.timeSinceLevelLoad;
            accelerating = true;
        } else if (velocity.magnitude == 0)
        {
            accelerating = false;
            //deaccelerating = true;
            isAtFullSpeed = false;
            startMovementTime = -1;
        }

        float movementSpeedMultiplier = 1;

        if (accelerating && !isAtFullSpeed && Time.timeSinceLevelLoad - startMovementTime > accelerateTime)
        {
            accelerating = false;
            isAtFullSpeed = true;
            Debug.Log(Time.timeSinceLevelLoad - startMovementTime);
        } else if (accelerating)
        {
            movementSpeedMultiplier = Accelerate(startMovementTime, accelerateTime);
        }

        characterController.Move(velocity * movementSpeedMultiplier);
    }


    public float Accelerate(float startingTime, float timeTillCompletion)
    {
        //return (0.5f * Mathf.Cos(((Time.timeSinceLevelLoad - startingTime - timeTillCompletion) * Mathf.PI) / timeTillCompletion) + 0.5f);
        return (Time.timeSinceLevelLoad - startingTime) / timeTillCompletion;
    }

    public float ReverseAccelerate(float currentSpeed, float maxSpeed)
    {
        return (1);
    }

    public float Deaccelerate(float startingTime, float timeTillCompletion)
    {
        return 1 - (0.5f * Mathf.Cos(((Time.timeSinceLevelLoad - startingTime - timeTillCompletion) * Mathf.PI) / timeTillCompletion) + 0.5f);
    }
}
