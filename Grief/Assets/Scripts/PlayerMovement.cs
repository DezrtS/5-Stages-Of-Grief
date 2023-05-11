using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;

    [SerializeField] private float maxSpeed = 15;
    [SerializeField] private float accelerateTime = 5f;
    [SerializeField] private float deaccelerateTime = 10f;

    private float startTime = -1;

    bool accelerating = false;
    bool deaccelerating = false;

    private float gravity;
    private Vector3 velocity;
    private Vector3 stopVelocity;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }


    void Update()
    {
        float xInput = Input.GetAxisRaw("Horizontal");
        float zInput = Input.GetAxisRaw("Vertical");

        Vector3 movement = (transform.right * xInput + transform.forward * zInput).normalized * maxSpeed;

        if (movement.magnitude == 0 && velocity.magnitude != 0 && !deaccelerating)
        {
            accelerating = false;
            deaccelerating = true;
            startTime = Time.timeSinceLevelLoad - (deaccelerateTime - ReverseAccelerate(velocity.magnitude, deaccelerateTime));
            stopVelocity = velocity;
            //Debug.Log($"Deaccelerating - Time To Deacceleration: {ReverseAccelerate(velocity.magnitude, deccelerateTime)}");
        }

        if (movement.magnitude > 0 && velocity.magnitude < maxSpeed && !accelerating)
        {
            deaccelerating = false;
            accelerating = true;
            startTime = Time.timeSinceLevelLoad - ReverseAccelerate(velocity.magnitude, accelerateTime);
            //Debug.Log($"Accelerating - Time To Accelerate: {accelerateTime - ReverseAccelerate(velocity.magnitude, accelerateTime)}");
        }

        float movementMultiplier = 1;

        if (accelerating && Time.timeSinceLevelLoad - startTime < accelerateTime)
        {
            //Debug.Log("Accelerating");
            movementMultiplier = Accelerate(startTime, accelerateTime);
        } 
        else if (deaccelerating && Time.timeSinceLevelLoad - startTime < deaccelerateTime)
        {
            //Debug.Log("Deaccelerating");
            movement = stopVelocity;
            movementMultiplier = 1 - Accelerate(startTime, deaccelerateTime);
        } 
        else if (accelerating || deaccelerating)
        {
            accelerating = false;
            deaccelerating = false;
            stopVelocity = Vector3.zero;
        }

        characterController.Move(movement * movementMultiplier * Time.deltaTime);
        velocity = movement * movementMultiplier;
    }


    public float Accelerate(float startingTime, float timeTillCompletion)
    {
        //return (0.5f * Mathf.Cos(((Time.timeSinceLevelLoad - startingTime - timeTillCompletion) * Mathf.PI) / timeTillCompletion) + 0.5f);
        return (Time.timeSinceLevelLoad - startingTime) / timeTillCompletion;
    }

    public float ReverseAccelerate(float currentSpeed, float timeTillCompletion)
    {
        return (currentSpeed / maxSpeed) * timeTillCompletion;
    }
}
