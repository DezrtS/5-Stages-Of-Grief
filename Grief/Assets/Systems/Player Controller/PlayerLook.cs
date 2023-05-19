using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    private GameObject cameraFollowObject;

    [SerializeField] private float panStrenght = 5;

    private void Awake()
    {
        cameraFollowObject = GameObject.Find("Camera Follow");
    }

    public void LookTowards(Transform player, Vector2 direction)
    {
        Vector3 camPos = player.position + PlayerMovement.movementAxis * direction * panStrenght;

        cameraFollowObject.transform.position = camPos;
    } 
}
