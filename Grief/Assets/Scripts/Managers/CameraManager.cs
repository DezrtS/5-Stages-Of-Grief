using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    public static Vector3 globalCameraOffset = new(-50, 50 * Mathf.Sqrt(2), -50);
    public Transform cameraTransform { get; private set; }

    private CinemachineVirtualCamera cinemachineCamera;
    private CinemachineTransposer cinemachineTransposer;

    private CinemachineBasicMultiChannelPerlin channelPerlin;

    private float shakeTimer;
    private bool isShaking;

    private bool foundCamera = false;

    protected override void Awake()
    {
        base.Awake();

        GameObject cameraGameObject = GameObject.Find("Cinemachine Camera");
        foundCamera = cameraGameObject != null;

        if (foundCamera)
        {
            cameraTransform = cameraGameObject.transform;
            cinemachineCamera = cameraGameObject.GetComponent<CinemachineVirtualCamera>();
            cinemachineTransposer = cinemachineCamera.GetCinemachineComponent<CinemachineTransposer>();
            channelPerlin = cinemachineCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            StopShake();

            if (cinemachineTransposer == null)
            {
                Debug.LogError("Cinemachine Camera was found however, the camera is currently not set to Transposer [Set the Body to \"Transpose\", the Binding Mode to \"World Space\", and the Follow Offset to Vector3.Zero]");
                foundCamera = false;
            }
        } 
        else
        {
            Debug.LogError("Cinemachine Camera was not found, [Make sure the cinemachine camera is named \"Cinemachine Camera\"]");
        }
    }

    public void TransferCameraTo(Transform transform)
    {
        if (foundCamera)
        {
            cinemachineCamera.Follow = null;
            cinemachineCamera.gameObject.transform.position = transform.position + globalCameraOffset;
            cinemachineCamera.Follow = transform;
        }
    }

    public void SetCameraDamping(float xDamp, float yDamp, float zDamp)
    {
        if (foundCamera)
        {
            cinemachineTransposer.m_XDamping = xDamp;
            cinemachineTransposer.m_YDamping = yDamp;
            cinemachineTransposer.m_ZDamping = zDamp;
        }
    }

    public void SetCameraOffset(Vector3 offset)
    {
        if (foundCamera)
        {
            cinemachineTransposer.m_FollowOffset = offset + globalCameraOffset;
        }
    }

    // Use Cached Camera
    //public Vector3 ConvertScreenToWorld(Vector3 screenPosition)
    //{
    //    return Camera.main.ScreenToWorldPoint(screenPosition);
    //}

    public void Shake(float shakeIntensity, float shakeTime)
    {
        if (foundCamera)
        {
            isShaking = true;
            channelPerlin.m_AmplitudeGain = shakeIntensity;
            shakeTimer = shakeTime;
        }
    }

    public void StopShake()
    {
        if (foundCamera)
        {
            isShaking = false;
            channelPerlin.m_AmplitudeGain = 0;
            shakeTimer = 0;
        }
    }

    private void Update()
    {
        if (isShaking)
        {
            shakeTimer -= Time.deltaTime;

            if (shakeTimer <= 0)
            {
                StopShake();
            }
        }
    }
}