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
            navMeshAgent.updatePosition = false;
        }
    }

    private void FixedUpdate()
    {
        if (isPathfinding)
        {
            UpdatePathfinding();
        } 
        //else
        //{
        //    rig.velocity = velocity;
        //}
    }

    // ---------------------------------------------------------------------------------------------------------
    // Base Abstract Methods
    // ---------------------------------------------------------------------------------------------------------

    public override Vector3 GetVelocity()
    {
        return navMeshAgent.velocity;
    }

    public override void SetVelocity(Vector3 velocity)
    {
        navMeshAgent.velocity = velocity;
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
    }

    public override void ApplyForce(Vector3 force)
    {
        navMeshAgent.velocity += force;
        //velocity += force;
    }

    public override void InitiatePathfinding(Transform transform, Vector3 destination)
    {
        if (CanInitiatePathfinding(transform.position))
        {
            pathfinder.TransferToPathfindingState(true);

            navMeshAgent.updatePosition = false;
            //navMeshAgent.enabled = true;

            isPathfinding = true;
            navMeshAgent.isStopped = false;
            navMeshAgent.destination = destination;

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

        transform.position = Vector3.SmoothDamp(navMeshAgent.nextPosition, navMeshAgent.nextPosition + navMeshAgent.velocity * Time.deltaTime, ref velocity, 0.1f);

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

        navMeshAgent.isStopped = true;
        navMeshAgent.updatePosition = true;

        pathfinder.TransferToPathfindingState(false);

        SetRotation(Quaternion.Inverse(MovementAxis) * transform.forward);
        //velocity = navMeshAgent.velocity;

        isPathfinding = false;

        SetAllowMovementInput(true);
        SetAllowRotationInput(true);
    }
}