using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/State Movement Data")]
public class StateMovementData : ScriptableObject
{
    [Header("Speed Variables")]
    [SerializeField] private float defaultSpeed = 0;
    [SerializeField] private float activeSpeed;

    [Space(10)]
    [Header("Direction Variables")]
    [SerializeField] private Vector3 defaultDirection = Vector3.forward;
    [SerializeField] private Vector3 activeDirection;

    [Space(10)]
    [Header("Activation Variables")]
    [Range(0, 1)]
    [SerializeField] private float activationStart = 0;
    [Range(0, 1)]
    [SerializeField] private float activationEnd = 1;

    [Space(10)]
    [Header("Acceleration Variables")]
    [SerializeField] private bool accelerateToDefaultSpeed;
    [Range(0, 1)]
    [SerializeField] private float lengthOfDefaultAcceleration = 1;
    [SerializeField] private bool accelerateToActiveSpeed;
    [Range(0, 1)]
    [SerializeField] private float lengthOfActiveAcceleration = 1;

    public Vector3 GetStateCurrentVelocity(float timeSinceStateStart, float stateTimeLength, Transform transform, Vector3 currentVelocity)
    {
        if (stateTimeLength == 0)
        {
            return Vector3.zero;
        }

        float stateCompletionPercentage = Mathf.Clamp(timeSinceStateStart / stateTimeLength, 0, 1);

        Vector3 targetVelocity = transform.rotation * defaultDirection * defaultSpeed;
        Vector3 velocity;

        if (activationStart <= stateCompletionPercentage && activationEnd >= stateCompletionPercentage)
        {
            targetVelocity = transform.rotation * activeDirection * activeSpeed;

            if (accelerateToActiveSpeed)
            {
                float accelerationProgress = Mathf.Clamp((stateCompletionPercentage - activationStart) / ((activationEnd - activationStart) * lengthOfActiveAcceleration), 0, 1);
                velocity = Vector3.Lerp(currentVelocity, targetVelocity, accelerationProgress);
            }
            else
            {
                velocity = targetVelocity;
            }
        }
        else
        {
            if (accelerateToDefaultSpeed)
            {
                float accelerationProgress = 1;

                if (stateCompletionPercentage < activationStart)
                {
                    accelerationProgress = Mathf.Clamp(stateCompletionPercentage / (activationStart * lengthOfDefaultAcceleration), 0, 1);
                } 
                else if (stateCompletionPercentage > activationEnd)
                {
                    accelerationProgress = Mathf.Clamp((stateCompletionPercentage - activationEnd) / ((1 - activationEnd) * lengthOfDefaultAcceleration), 0, 1);
                }

                velocity = Vector3.Lerp(currentVelocity, targetVelocity, accelerationProgress);
            }
            else
            {
                velocity = targetVelocity;
            }
        }

        return velocity;
    }
}