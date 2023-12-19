using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/State Movement")]
public class StateMovement : ScriptableObject
{
    [SerializeField] private List<StateMovementData> stateMovementData = new();

    public Vector3 GetStateCurrentVelocity(float timeSinceStateStart, float stateTimeLength, Transform transform, Vector3 currentVelocity)
    {
        if (stateTimeLength == 0)
        {
            return Vector3.zero;
        }

        float stateCompletionPercentage = Mathf.Clamp(timeSinceStateStart / stateTimeLength, 0, 1);

        Vector3 velocity = Vector3.zero;

        for (int i = stateMovementData.Count - 1; i >= 0; i--)
        {
            if (stateCompletionPercentage >= stateMovementData[i].ActivationRangeStart && stateCompletionPercentage <= stateMovementData[i].ActivationRangeEnd)
            {
                velocity = stateMovementData[i].GetStateCurrentVelocity(stateCompletionPercentage, transform, currentVelocity);
                break;
            }
        }

        return velocity;
    }
}

[Serializable]
public class StateMovementData
{
    [Header("Speed & Direction Variables")]
    [SerializeField] private float activeSpeed;
    [SerializeField] private Vector3 activeDirection;

    [Header("Activation Range Variables")]
    [Range(0, 1)]
    [SerializeField] private float activationRangeStart = 0;
    [Range(0, 1)]
    [SerializeField] private float activationRangeEnd = 1;

    [Header("Acceleration Variables")]
    [SerializeField] private bool accelerateToActiveSpeed;
    [Range(0, 1)]
    [SerializeField] private float rangeOfAcceleration = 1;

    public float ActivationRangeStart { get { return activationRangeStart; } }
    public float ActivationRangeEnd { get {  return activationRangeEnd; } }

    public Vector3 GetStateCurrentVelocity(float stateCompletionPercentage, Transform transform, Vector3 currentVelocity)
    {
        Vector3 velocity;

        Vector3 targetVelocity = transform.rotation * activeDirection * activeSpeed;

        if (accelerateToActiveSpeed)
        {
            float accelerationProgress = Mathf.Clamp((stateCompletionPercentage - activationRangeStart) / ((activationRangeEnd - activationRangeStart) * rangeOfAcceleration), 0, 1);
            velocity = Vector3.Lerp(currentVelocity, targetVelocity, accelerationProgress);
        }
        else
        {
            velocity = targetVelocity;
        }

        return velocity;
    }
}