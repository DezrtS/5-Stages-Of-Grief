using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class RigidTransform : MonoBehaviour
{
    public static Quaternion MovementAxis = Quaternion.Euler(90, 0, -45);
    public static float Gravity = 19.6f;

    [Header("Movement Variables")]
    [SerializeField] private float maxSpeed = 15;
    [Range(0, 100)]
    [SerializeField] private float totalAccelerationTime = 0.25f;
    [Range(0, 100)]
    [SerializeField] private float totalDeaccelerationTime = 0.5f;
    [SerializeField] private float inputSmoothMultiplier = 6;

    private float speed = 0;
    private Vector3 velocity = Vector3.zero;
    private Vector3 rotation = Vector3.right;

    private bool canMove = true;
    private bool canRotate = true;

    private CharacterController characterController;
    private IMove inputProvider;

    private Vector2 previousInput = Vector2.zero;

    public float Speed { get { return speed; } }
    public Vector3 Velocity { get { return velocity; } set { velocity = value; } }
    public Vector3 Rotation { get { return rotation; } }



    private float gravityVelocity = 0;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        if (!TryGetComponent(out inputProvider))
        {
            Debug.LogWarning($"{name} does not provide input");
        }
    }

    private void FixedUpdate()
    {
        UpdateMovement();
        UpdateRotation();
        //ApplyGravity(); - Has some bugs
        CheckCollisions();
    }

    private void UpdateMovement()
    {
        Vector3 input = Vector3.zero;

        speed = velocity.magnitude;

        if (canMove && inputProvider != null)
        {
            input = inputProvider.GetMovementInput();
        }

        Vector2 smoothedInput = Vector2.Lerp(previousInput, input, Time.deltaTime * inputSmoothMultiplier);
        previousInput = smoothedInput;

        Vector3 targetDirection = MovementAxis * smoothedInput.normalized;
        Vector3 targetVelocity = maxSpeed * smoothedInput.magnitude * targetDirection;
        float targetSpeed = targetVelocity.magnitude;

        Vector3 velocityDifference = targetVelocity - velocity;
        Vector3 differenceDirection = velocityDifference.normalized;

        if (speed < targetSpeed)
        {
            float accelerationIncrement = GetAcceleration(maxSpeed, totalAccelerationTime) * Time.deltaTime;

            if (velocityDifference.magnitude < accelerationIncrement)
            {
                velocity = targetVelocity;
            }
            else
            {
                velocity += differenceDirection * accelerationIncrement;
            }

            speed = velocity.magnitude;

        }
        else if (speed > targetSpeed)
        {
            float deaccelerationIncrement = GetAcceleration(maxSpeed, totalDeaccelerationTime) * Time.deltaTime;

            if (velocityDifference.magnitude < deaccelerationIncrement)
            {
                velocity = targetVelocity;
            }
            else
            {
                velocity += differenceDirection * deaccelerationIncrement;
            }

            speed = velocity.magnitude;
        }

        characterController.Move(velocity * Time.deltaTime);

        if (targetDirection != Vector3.zero)
        {
            rotation = targetDirection;
        }
    }

    private void UpdateRotation()
    {
        if (rotation != Vector3.zero && canRotate)
        {
            transform.forward = rotation;
        }
    }

    public void ForceRotation(Vector3 rotation)
    {
        this.rotation = rotation;
        transform.forward = rotation;
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded)
        {
            gravityVelocity = 0.5f;
        }

        gravityVelocity += Gravity * Time.deltaTime;
        characterController.Move(Vector2.down * gravityVelocity * Time.deltaTime);
    }

    private void CheckCollisions()
    {
        // Needs to be updated to adjust for larger models and for avoiding different collisions / collision layers

        RaycastHit raycastHit;
        if (Physics.Raycast(transform.position, velocity.normalized, out raycastHit, 0.75f))
        {
            //Debug.Log("Raycast Hit");

            Vector3 normal = raycastHit.normal;

            float projectionMagnitude = Vector3.Dot(velocity, normal);
            Vector3 projection = projectionMagnitude * normal;

            Vector3 newVelocity = velocity - projection;

            velocity = newVelocity;

            speed = velocity.magnitude;
        }
    }

    public float GetAcceleration(float maxSpeed, float timeToReachFullSpeed)
    {
        return (maxSpeed) / timeToReachFullSpeed;
    }

    public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;
    }

    public void SetCanRotate(bool canRotate)
    {
        this.canRotate = canRotate;
    }
}