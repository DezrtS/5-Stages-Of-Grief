using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    private CinemachineVirtualCamera cinemachineCamera;
    private CinemachineTransposer cinemachineTransposer;

    private bool foundCamera = false;

    protected override void Awake()
    {
        base.Awake();

        GameObject cameraGameObject = GameObject.Find("Cinemachine Camera");
        foundCamera = cameraGameObject != null;

        if (foundCamera)
        {
            cinemachineCamera = cameraGameObject.GetComponent<CinemachineVirtualCamera>();
            cinemachineTransposer = cinemachineCamera.GetCinemachineComponent<CinemachineTransposer>();

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
            cinemachineCamera.gameObject.transform.position = transform.position;
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
            cinemachineTransposer.m_FollowOffset = offset;
        }
    }
}