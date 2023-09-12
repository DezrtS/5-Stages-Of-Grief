using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Dodges/Basic Player Dodge")]
public class BasicPlayerDodge : Dodge
{
    public override void OnDodgeStart(Vector3 dodgeDirection, Vector3 directionInput)
    {
        base.OnDodgeStart(dodgeDirection, directionInput);

        parentMovementController.SetVelocity(Vector3.zero);
        parentMovementController.SetRotation(dodgeDirection);
    }

    public override void OnDodge(Vector3 dodgeDirection, float timeSinceDodgeStarted)
    {
        base.OnDodge(dodgeDirection, timeSinceDodgeStarted);

        parentMovementController.SetVelocity(MovementController.MovementAxis * dodgeDirection * GetDodgeSpeed());
    }
}
