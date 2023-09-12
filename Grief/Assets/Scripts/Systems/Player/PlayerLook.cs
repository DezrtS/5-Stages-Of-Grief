using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    private CameraManager cameraManager;

    private Vector3 currentOffset = Vector2.zero;

    private void Start()
    {
        cameraManager = CameraManager.Instance;
    }

    public void PanTowards(Vector2 input, float panStrength, float panTimeMultiplier)
    {
        Vector3 targetOffset = MovementController.MovementAxis * input * panStrength;
        Vector3 direction = targetOffset - currentOffset;
        Vector3 increment = (direction).normalized / panTimeMultiplier;

        if (direction.magnitude <= increment.magnitude)
        {
            currentOffset = targetOffset;
        } 
        else
        {
            currentOffset += increment;
        }

        cameraManager.SetCameraOffset(currentOffset);

        //Vector2 smoothedInput = Vector2.Lerp(previousInput, input, Time.deltaTime * inputSmoothMultiplier);
        //previousInput = smoothedInput;
        //cameraManager.SetCameraOffset(PlayerMovement.movementAxis * smoothedInput * panStrength);
    } 
}