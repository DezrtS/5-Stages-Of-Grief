using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovementController : MonoBehaviour
{
    public static Quaternion MovementAxis = Quaternion.Euler(90, 0, -45);

    public abstract Vector3 GetVelocity();
    public abstract void SetVelocity(Vector3 velocity);
    public abstract Vector3 GetRotation();
    public abstract void SetRotation(Vector3 rotation);

    public abstract void SetAllowMovementInput(bool isAllowed);
    public abstract void SetAllowRotationInput(bool isAllowed);

    public abstract void ApplyForce(Vector3 force);

    public abstract void Teleport(Vector3 location);

    public abstract void InitiatePathfinding(Transform transform, Vector3 destination);
    public abstract bool CanInitiatePathfinding(Vector3 position);
    protected abstract void UpdatePathfinding();
    protected abstract void CheckPathfindingState();
    public abstract void StopPathfinding();


}