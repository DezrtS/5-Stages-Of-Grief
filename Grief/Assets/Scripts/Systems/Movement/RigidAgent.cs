using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class RigidAgent : MovementController
{
    // ---------------------------------------------------------------------------------------------------------
    // Movement Variables
    // ---------------------------------------------------------------------------------------------------------

    private Rigidbody rig;

    [SerializeField] private float runningSpeed = 10;
    [SerializeField] private float walkingSpeed = 5;

    private Vector3 velocity = Vector3.zero;

    private bool allowMovementInput = true;
    private bool allowRotationInput = true;

    IAnimate animator;
    private bool isWalking;
    private bool isRunning;

    // ---------------------------------------------------------------------------------------------------------
    // Pathfinding Variables
    // ---------------------------------------------------------------------------------------------------------

    private bool isPathfinding;

    private NavMeshAgent navMeshAgent;
    private IPathfind pathfinder;

    // ---------------------------------------------------------------------------------------------------------
    // Default Unity Methods
    // ---------------------------------------------------------------------------------------------------------

    private void Start()
    {
        rig = GetComponent<Rigidbody>();

        TryGetComponent(out navMeshAgent);
        TryGetComponent(out pathfinder);
        TryGetComponent(out animator);

        if (navMeshAgent != null)
        {
            navMeshAgent.updatePosition = false;
        }
    }

    private void FixedUpdate()
    {
        if (isPathfinding)
        {
            UpdatePathfinding();
        }

        UpdateAnimations();
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
        else
        {
            return rig.velocity;
        }
    }

    public override void SetVelocity(Vector3 velocity)
    {
        if (isPathfinding)
        {
            navMeshAgent.velocity = velocity;
        }
        else
        {
            rig.velocity = velocity;
        }
    }

    public override Vector3 GetRotation()
    {
        return transform.forward;
    }

    public override void SetRotation(Vector3 rotation)
    {
        transform.forward = MovementAxis * rotation.normalized;
    }

    public override void SetAllowMovementInput(bool isAllowed)
    {
        allowMovementInput = isAllowed;
    }

    public override void SetAllowRotationInput(bool isAllowed)
    {
        allowRotationInput = isAllowed;

        if (navMeshAgent != null)
        {
            navMeshAgent.updateRotation = allowRotationInput;
        }
    }

    public override void ApplyForce(Vector3 force)
    {
        if (isPathfinding)
        {
            navMeshAgent.velocity += force;
        }
        else
        {
            rig.velocity += force;
        }
    }

    public override void Teleport(Vector3 location)
    {
        throw new System.NotImplementedException();
    }

    public override void InitiatePathfinding(Transform transform, Vector3 destination)
    {
        if (CanInitiatePathfinding(transform.position))
        {
            pathfinder.TransferToPathfindingState(true);

            navMeshAgent.updatePosition = false;
            navMeshAgent.enabled = true;

            isPathfinding = true;
            navMeshAgent.isStopped = false;
            navMeshAgent.destination = destination;
            navMeshAgent.velocity = velocity;
            rig.velocity = Vector3.zero;

            rig.isKinematic = true;

            SetAllowMovementInput(false);
            //SetAllowRotationInput(false);
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

        transform.position = Vector3.SmoothDamp(navMeshAgent.nextPosition, navMeshAgent.nextPosition + navMeshAgent.velocity * Time.deltaTime, ref velocity, 0.1f);

        navMeshAgent.destination = pathfinder.PathfindDestination;

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
        navMeshAgent.updatePosition = true;

        pathfinder.TransferToPathfindingState(false);

        rig.isKinematic = false;

        SetRotation(Quaternion.Inverse(MovementAxis) * transform.forward);
        rig.velocity = navMeshAgent.velocity;

        navMeshAgent.enabled = false;

        isPathfinding = false;

        SetAllowMovementInput(true);
        SetAllowRotationInput(true);
    }

    private void UpdateAnimations()
    {
        if (velocity.magnitude >= runningSpeed)
        {
            if (!isRunning)
            {
                //Debug.Log("IsNowRunning");
                animator?.OnAnimationStart(AnimationEvent.Run, "");
                isWalking = false;
                isRunning = true;
            }
        }
        else if (velocity.magnitude >= walkingSpeed)
        {
            if (!isWalking)
            {
                //Debug.Log("IsNowWalking");
                animator?.OnAnimationStart(AnimationEvent.Walk, "");
                isRunning = false;
                isWalking = true;
            }
        }
        else if (isWalking || isRunning)
        {
            //Debug.Log("IsNowStanding");
            animator?.OnAnimationStart(AnimationEvent.Stand, "");
            isWalking = false;
            isRunning = false;
        }
    }
}