using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/State Movement Data")]
public class StateMovementData : ScriptableObject
{
    [Header("Speed Variables")]
    public float defaultSpeed = 0;
    public float activeStartSpeed;
    public float activeEndSpeed;

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

    public Vector3 GetStateCurrentVelocity(float timeSinceStateStart, float stateTimeLength)
    {
        if (stateTimeLength == 0)
        {
            return defaultDirection * defaultSpeed;
        }

        float stateCompletionPercentage = timeSinceStateStart / stateTimeLength;

        float speed = defaultSpeed;

        if (true)
        {
            return Vector3.zero;
        }


    }
}