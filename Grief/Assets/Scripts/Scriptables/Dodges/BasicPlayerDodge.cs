using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Dodges/Basic Player Dodge")]
public class BasicPlayerDodge : Dodge
{
    public override void OnDodgeStart<T>(Vector3 dodgeDirection, T dodger)
    {
        base.OnDodgeStart(dodgeDirection, dodger);

        parentRigidTransform.Velocity = Vector3.zero;
        parentRigidTransform.ForceRotation(dodgeDirection);
    }

    public override void OnDodge<T>(Vector3 dodgeDirection, T dodger, float timeSinceDodgeStarted)
    {
        base.OnDodge(dodgeDirection, dodger, timeSinceDodgeStarted);

        parentRigidTransform.Velocity = dodgeDirection * dodgeSpeed;
    }
}
