using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footstep : MonoBehaviour
{
    [SerializeField] private MovementController movementController;

    public void TriggerFootstep(int foot)
    {
        if (movementController != null)
        {
            movementController.TriggerFootstep(foot);
        }
    }
}
