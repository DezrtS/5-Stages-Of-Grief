using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbienceChangeTrigger : MonoBehaviour
{
    [SerializeField] private string parameterName;
    [Range(0,1)]
    [SerializeField] private float enterParameterValue;
    [Range(0, 1)]
    [SerializeField] private float exitParameterValue;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AudioManager.Instance.SetMusicPerameter(parameterName, enterParameterValue);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AudioManager.Instance.SetMusicPerameter(parameterName, exitParameterValue);
        }
    }
}
