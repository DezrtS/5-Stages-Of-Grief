using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDodge : MonoBehaviour
{
    private float timeSinceLastDodge = Mathf.NegativeInfinity;

    public Vector3 InitiateDodge(Transform transform, Vector2 input)
    {
        Vector3 dodgeDirection = transform.forward;

        if (input.magnitude != 0)
        {
            dodgeDirection = PlayerMovement.movementAxis * input.normalized;
        }

        return dodgeDirection;
    }

    public bool CanDodge(Dodge dodge)
    {
        return (Time.timeSinceLevelLoad - timeSinceLastDodge >= dodge.DodgeCooldown);
    }

    public void HandleDodge(CharacterController characterController, Vector3 dodgeDirection, Dodge dodge)
    {
        characterController.Move(dodge.DodgeSpeed * dodgeDirection * Time.deltaTime);
    }
}
