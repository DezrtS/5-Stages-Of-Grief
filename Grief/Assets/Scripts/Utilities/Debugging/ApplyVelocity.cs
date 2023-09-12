using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyVelocity : MonoBehaviour
{
    [SerializeField] private Vector3 force;
    [SerializeField] private bool applyForce;

    private MovementController movementController;

    void Start()
    {
        movementController = GetComponent<MovementController>();
    }

    void FixedUpdate()
    {
        if (applyForce)
        {
            applyForce = false;
            ApplyForce();
        }
    }

    public void ApplyForce()
    {
        movementController.ApplyForce(force);
    }
}
