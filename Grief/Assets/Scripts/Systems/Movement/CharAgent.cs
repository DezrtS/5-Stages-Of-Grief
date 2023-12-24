using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterController))]
public class CharAgent : MovementController
{
    public static float Gravity = 19.6f;

    // ---------------------------------------------------------------------------------------------------------
    // Movement Variables
    // ---------------------------------------------------------------------------------------------------------

    [Header("Movement")]
    [Space(5)]
    [SerializeField] private float maxSpeed = 15;
    [Range(0, 100)]
    [SerializeField] private float totalAccelerationTime = 0.25f;
    [Range(0, 100)]
    [SerializeField] private float totalDeaccelerationTime = 0.5f;

    [Space(10)]
    [SerializeField] private bool useMovementInputSmooth = true;
    [SerializeField] private float movementInputSmoothMultiplier = 6;
    [SerializeField] private bool useRotationInputSmooth = true;
    [SerializeField] private float rotationInputSmoothMultiplier = 6;

    [Space(10)]
    [Header("Gravity")]
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float groundCheckDistance;

    private Vector3 velocity = Vector3.zero;
    private Vector3 rotation = Vector3.right;

    private bool allowMovementInput = true;
    private bool allowRotationInput = true;

    private CharacterController characterController;
    private IMove inputProvider;

    private Vector2 previousMovementInput = Vector2.zero;
    private Vector3 previousRotationInput = Vector3.zero;

    private float gravityVelocity = 0;

    private EventInstance footsteps;
    private string groundTag;

    // ---------------------------------------------------------------------------------------------------------
    // Pathfinding Variables
    // ---------------------------------------------------------------------------------------------------------

    private NavMeshAgent navMeshAgent;
    private IPathfind pathfinder;

    private bool isPathfinding;

    // ---------------------------------------------------------------------------------------------------------
    // Default Unity Methods
    // ---------------------------------------------------------------------------------------------------------

    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        TryGetComponent(out inputProvider);
        TryGetComponent(out navMeshAgent);
        TryGetComponent(out pathfinder);

        if (navMeshAgent != null)
        {
            navMeshAgent.updatePosition = false;
            navMeshAgent.enabled = false;
        }

        footsteps = AudioManager.Instance.CreateInstance(FMODEventsManager.Instance.footsteps);
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(footsteps, transform);
    }

    private void FixedUpdate()
    {
        if (!isPathfinding)
        {
            UpdateMovement();
            UpdateRotation();
            //CheckCollisions();
            ApplyGravity();
        }
        else
        {
            UpdatePathfinding();
        }

        UpdateSound();
    }

    // ---------------------------------------------------------------------------------------------------------
    // Class Methods
    // ---------------------------------------------------------------------------------------------------------

    private void UpdateMovement()
    {
        Vector3 movementInput = Vector3.zero;
        Vector3 rotationInput = previousRotationInput;

        if (inputProvider != null)
        {
            if (allowMovementInput)
            {
                movementInput = inputProvider.GetMovementInput();
            }

            Vector3 tempRotationInput = inputProvider.GetRotationInput();

            if (allowRotationInput && tempRotationInput != Vector3.zero)
            {
                rotationInput = tempRotationInput;
            }
        }


        Vector2 smoothedMovementInput = movementInput;
        if (useMovementInputSmooth)
        {
            smoothedMovementInput = Vector2.Lerp(previousMovementInput, movementInput, Time.deltaTime * movementInputSmoothMultiplier);
        }
        previousMovementInput = smoothedMovementInput;

        Vector3 smoothedRotationInput = rotationInput;
        if (useRotationInputSmooth)
        {
            smoothedRotationInput = Vector3.Lerp(previousRotationInput, rotationInput, Time.deltaTime * rotationInputSmoothMultiplier);
        }
        previousRotationInput = smoothedRotationInput;

        Vector3 targetRotation = MovementAxis * smoothedRotationInput.normalized;
        Vector3 targetVelocityDirection = MovementAxis * smoothedMovementInput.normalized;
        Vector3 targetVelocity = maxSpeed * smoothedMovementInput.magnitude * targetVelocityDirection;
        float targetSpeed = targetVelocity.magnitude;

        Vector3 velocityDifference = targetVelocity - velocity;
        Vector3 differenceDirection = velocityDifference.normalized;

        float accelerationIncrement;

        if (velocity.magnitude <= targetSpeed)
        {
            accelerationIncrement = GetAcceleration(maxSpeed, totalAccelerationTime) * Time.deltaTime;
        } 
        else
        {
            accelerationIncrement = GetAcceleration(maxSpeed, totalDeaccelerationTime) * Time.deltaTime;
        }

        if (velocityDifference.magnitude < accelerationIncrement)
        {
            velocity = targetVelocity;
        }
        else
        {
            velocity += differenceDirection * accelerationIncrement;
        }

        characterController.Move(velocity * Time.deltaTime);

        if (targetRotation != Vector3.zero)
        {
            rotation = targetRotation;
        }
    }

    private void UpdateRotation()
    {
        if (rotation != Vector3.zero && allowRotationInput)
        {
            transform.forward = rotation;
        }
    }

    private void ApplyGravity()
    {
        if (Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayerMask, QueryTriggerInteraction.Ignore))
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
        if (Physics.Raycast(transform.position, velocity.normalized, out raycastHit, 2f))
        {
            Debug.Log("Raycast Hit");

            Vector3 normal = raycastHit.normal;

            float projectionMagnitude = Vector3.Dot(velocity, normal);
            Vector3 projection = projectionMagnitude * normal;

            Vector3 newVelocity = velocity - projection;

            velocity = newVelocity;
        }
    }

    public float GetAcceleration(float maxSpeed, float timeToReachFullSpeed)
    {
        if (timeToReachFullSpeed == 0)
        {
            return maxSpeed;
        }

        return (maxSpeed) / timeToReachFullSpeed;
    }

    // ---------------------------------------------------------------------------------------------------------
    // Base Abstract Methods
    // ---------------------------------------------------------------------------------------------------------

    public override Vector3 GetVelocity()
    {
        if (isPathfinding)
        {
            return navMeshAgent.velocity;
        }

        return velocity;
    }

    public override void SetVelocity(Vector3 velocity)
    {
        if (isPathfinding)
        {
            navMeshAgent.velocity = velocity;
            return;
        } 
        
        this.velocity = velocity;
    }

    public override Vector3 GetRotation()
    {
        return transform.forward;
    }

    public override void SetRotation(Vector3 rotation)
    {
        previousRotationInput = rotation;
        transform.forward = MovementAxis * rotation;
    }

    public override void SetAllowMovementInput(bool isAllowed)
    {
        allowMovementInput = isAllowed;
    }

    public override void SetAllowRotationInput(bool isAllowed)
    {
        allowRotationInput = isAllowed;
    }

    public override void ApplyForce(Vector3 force)
    {
        if (isPathfinding)
        {
            navMeshAgent.velocity += force;
            return;
        }

        velocity += force;
    }

    public override void Teleport(Vector3 location)
    {
        if (isPathfinding)
        {
            transform.position = location;
            navMeshAgent.Warp(location);
        } 
        else
        {
            characterController.enabled = false;
            transform.position = location;
            characterController.enabled = true;
        }
    }

    public override void InitiatePathfinding(Transform transform, Vector3 destination)
    {
        if (CanInitiatePathfinding(transform.position))
        {
            pathfinder.TransferToPathfindingState(true);

            navMeshAgent.enabled = true;
            characterController.enabled = false;

            isPathfinding = true;
            navMeshAgent.isStopped = false;
            navMeshAgent.destination = destination;
            navMeshAgent.velocity = velocity;
            velocity = Vector3.zero;
            previousMovementInput = Vector2.zero;

            SetAllowMovementInput(false);
            SetAllowRotationInput(false);
        }
    }

    public override bool CanInitiatePathfinding(Vector3 position)
    {
        if (isPathfinding)
        {
            return false;
        }
        else if (navMeshAgent == null)
        {
            Debug.LogWarning($"{name} does not have a NavMeshAgent component");
            return false;
        }
        else if (pathfinder == null)
        {
            Debug.LogWarning($"{name} cannot pathfind");
            return false;
        }

        return NavMesh.SamplePosition(position, out _, navMeshAgent.height * 2f, NavMesh.AllAreas);
    }

    protected override void UpdatePathfinding()
    {
        pathfinder.OnPathfinding();

        navMeshAgent.destination = pathfinder.PathfindDestination;

        transform.position = Vector3.SmoothDamp(navMeshAgent.nextPosition, navMeshAgent.nextPosition + navMeshAgent.velocity * Time.deltaTime, ref velocity, 0.1f);

        CheckPathfindingState();
    }

    protected override void CheckPathfindingState()
    {
        if (navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid || (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && !navMeshAgent.pathPending))
        {
            StopPathfinding();
        }
    }

    public override void StopPathfinding()
    {
        if (!isPathfinding)
        {
            return;
        }

        navMeshAgent.isStopped = true;

        pathfinder.TransferToPathfindingState(false);

        SetRotation(Quaternion.Inverse(MovementAxis) * transform.forward);
        velocity = navMeshAgent.velocity;

        navMeshAgent.enabled = false;
        characterController.enabled = true;

        isPathfinding = false;

        SetAllowMovementInput(true);
        SetAllowRotationInput(true);
    }

    // Change to Play footstep sound at a certain rate
    private void UpdateSound()
    {
        if (velocity.magnitude > 0.75f)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, groundCheckDistance, groundLayerMask, QueryTriggerInteraction.Ignore))
            {
                string tag = hitInfo.collider.tag;

                if (tag != groundTag)
                {
                    groundTag = tag;

                    switch (tag)
                    {
                        case "Snow":
                            footsteps.setParameterByName("terrain_material", 1);
                            break;
                        case "Ice":
                            footsteps.setParameterByName("terrain_material", 2);
                            break;
                        default:
                            footsteps.setParameterByName("terrain_material", 0);
                            break;
                    }
                }
            }

            footsteps.getPlaybackState(out PLAYBACK_STATE playbackState);
            if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
            {
                footsteps.start();
                FMODUnity.RuntimeManager.AttachInstanceToGameObject(footsteps, transform);
            }
        } 
        else
        {
            footsteps.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }
}