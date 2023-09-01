using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

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
    private bool isPathfinding;

    private CharacterController characterController;
    private IMove inputProvider;

    private NavMeshAgent agent;
    private IPathfind pathfinder;

    private Vector2 previousMovementInput = Vector2.zero;
    private Vector3 previousRotationInput = Vector3.zero;

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

        TryGetComponent(out agent);
        TryGetComponent(out pathfinder);
    }

    private void FixedUpdate()
    {
        if (!isPathfinding)
        {
            UpdateMovement();
            UpdateRotation();
            //CheckCollisions();
        }
        else
        {
            CheckPathfindingState();
        }

        //ApplyGravity(); - Currently has some bugs
    }

    private void UpdateMovement()
    {
        Vector3 movementInput = Vector3.zero;
        Vector3 rotationInput = previousRotationInput;

        speed = velocity.magnitude;

        if (inputProvider != null)
        {
            if (canMove)
            {
                movementInput = inputProvider.GetMovementInput();
            }

            Vector3 tempRotationInput = inputProvider.GetRotationInput();

            if (canRotate && tempRotationInput != Vector3.zero)
            {
                rotationInput = tempRotationInput;
            }
        }

        Vector2 smoothedMovementInput = Vector2.Lerp(previousMovementInput, movementInput, Time.deltaTime * inputSmoothMultiplier);
        previousMovementInput = smoothedMovementInput;

        Vector3 smoothedRotationInput = Vector3.Lerp(previousRotationInput, rotationInput, Time.deltaTime * inputSmoothMultiplier);
        previousRotationInput = smoothedRotationInput;

        Vector3 targetDirection = MovementAxis * smoothedRotationInput.normalized;
        Vector3 targetVelocity = maxSpeed * smoothedMovementInput.magnitude * targetDirection;
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
        previousRotationInput = rotation;
        transform.forward = MovementAxis * rotation;
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

    public void InitiatePathfinding(Vector3 destination, Transform transform)
    {
        if (CanInitiatePathfinding(transform.position))
        {
            pathfinder.TransferToPathfindingState(true);

            agent.enabled = true;
            characterController.enabled = false;

            isPathfinding = true;
            agent.isStopped = false;
            agent.destination = destination;
            agent.velocity = velocity;
            velocity = Vector3.zero;
            previousMovementInput = Vector2.zero;

            SetCanRotate(false);
            SetCanMove(false);
        }
    }

    public bool CanInitiatePathfinding(Vector3 position)
    {
        if (agent == null)
        {
            Debug.LogWarning($"{name} does not have a NavMeshAgent component");
            return false;
        } 
        else if (pathfinder == null)
        {
            Debug.LogWarning($"{name} cannot pathfind");
            return false;
        }

        return NavMesh.SamplePosition(position, out _, 2f, NavMesh.AllAreas) && !isPathfinding;
    }

    private void CheckPathfindingState()
    {
        if (agent.pathStatus == NavMeshPathStatus.PathInvalid || agent.remainingDistance <= agent.stoppingDistance)
        {
            StopPathfinding();
        }
    }

    public void StopPathfinding()
    {
        if (!isPathfinding)
        {
            return;
        }

        pathfinder.TransferToPathfindingState(false);

        ForceRotation(Quaternion.Inverse(MovementAxis) * transform.forward);
        velocity = agent.velocity;
        agent.isStopped = true;

        agent.enabled = false;
        characterController.enabled = true;

        isPathfinding = false;

        SetCanRotate(true);
        SetCanMove(true);
    }
}