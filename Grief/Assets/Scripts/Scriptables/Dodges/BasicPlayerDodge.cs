using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Dodges/Basic Player Dodge")]
public class BasicPlayerDodge : Dodge
{
    public override void OnDodgeStart(Vector3 dodgeDirection, Vector3 directionInput)
    {
        base.OnDodgeStart(dodgeDirection, directionInput);

        parentRigidTransform.Velocity = Vector3.zero;
        parentRigidTransform.ForceRotation(dodgeDirection);
    }

    public override void OnDodge(Vector3 dodgeDirection, float timeSinceDodgeStarted)
    {
        base.OnDodge(dodgeDirection, timeSinceDodgeStarted);

        parentRigidTransform.Velocity = RigidTransform.MovementAxis * dodgeDirection * GetDodgeSpeed();
    }
}
