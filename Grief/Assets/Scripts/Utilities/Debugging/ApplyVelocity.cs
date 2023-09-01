using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyVelocity : MonoBehaviour
{
    [SerializeField] private Vector3 force;
    [SerializeField] private bool applyForce;

    private RigidTransform rigidTrans;

    void Start()
    {
        rigidTrans = GetComponent<RigidTransform>();
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
        rigidTrans.Velocity += force;
    }
}
