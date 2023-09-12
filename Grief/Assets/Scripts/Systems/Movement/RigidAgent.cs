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

    private Vector3 velocity = Vector3.zero;

    private bool allowMovementInput = true;
    private bool allowRotationInput = true;

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

        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = false;
            navMeshAgent.updatePosition = false;
        }
    }

    private void FixedUpdate()
    {
        if (isPathfinding)
        {
            UpdatePathfinding();
        }
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

        return rig.velocity;
    }

    public override void SetVelocity(Vector3 velocity)
    {
        if (isPathfinding)
        {
            navMeshAgent.velocity = velocity;
        }

        rig.velocity = velocity;
    }

    public override Vector3 GetRotation()
    {
        return transform.forward;
    }

    public override void SetRotation(Vector3 rotation)
    {
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
        }

        rig.velocity += force;
    }

    public override void InitiatePathfinding(Transform transform, Vector3 destination)
    {
        if (CanInitiatePathfinding(transform.position))
        {
            pathfinder.TransferToPathfindingState(PathingState.Pathfinding);

            navMeshAgent.enabled = true;

            isPathfinding = true;
            navMeshAgent.isStopped = false;
            navMeshAgent.destination = destination;
            navMeshAgent.velocity = GetVelocity();
            SetVelocity(Vector3.zero);

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

        transform.position = Vector3.SmoothDamp(transform.position, navMeshAgent.nextPosition, ref velocity, 0.1f);

        navMeshAgent.destination = pathfinder.PathfindDestination;

        CheckPathfindingState();
    }

    protected override void CheckPathfindingState()
    {
        if (navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid || navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
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

        pathfinder.TransferToPathfindingState(PathingState.Stopping);

        navMeshAgent.isStopped = true;

        StartCoroutine(EndPathfinding());
    }

    protected override IEnumerator EndPathfinding()
    {
        // Optimize This Method

        Vector3 originalDestination = pathfinder.PathfindDestination;

        while (true)
        {
            yield return new WaitForFixedUpdate();

            Vector2 entityPosition = new(transform.position.x, transform.position.z);
            Vector2 targetPosition = new(navMeshAgent.nextPosition.x, navMeshAgent.nextPosition.z);

            if (originalDestination != pathfinder.PathfindDestination)
            {
                navMeshAgent.isStopped = false;
                break;
            }

            if (entityPosition == targetPosition)
            {
                pathfinder.TransferToPathfindingState(PathingState.Idle);

                SetRotation(Quaternion.Inverse(MovementAxis) * transform.forward);
                SetVelocity(navMeshAgent.velocity);

                navMeshAgent.enabled = false;

                isPathfinding = false;

                SetAllowMovementInput(true);
                SetAllowRotationInput(true);

                break;
            }
        }
    }
}