using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/State Movement Data")]
public class StateMovementData : ScriptableObject
{
    [Header("Velocity Variables")]
    public float defaultVelocity = 0;
    public float activeStartVelocity;
    public float activeEndVelocity;

    [Header("Direction Variables")]
    public Vector3 defaultDirection = Vector3.forward;
    public Vector3 activeStartDirection;
    public Vector3 activeEndDirection;

    [Header("Activation Variables")]
    [Range(0, 1)]
    public float activationStart = 0;
    [Range(0, 1)]
    public float activationEnd = 1;

    [Header("Acceleration Variables")]
    public bool useAcceleration;
    public float totalAccelerationTime;
}