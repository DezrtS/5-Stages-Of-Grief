using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodSplatterSpawner : MonoBehaviour
{
    [SerializeField] private GameObject bloodSplatter;
    GameObject activeSplatter = null;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (activeSplatter != null)
            {
                Destroy(activeSplatter);
            }

            activeSplatter = Instantiate(bloodSplatter, transform);
            activeSplatter.GetComponent<BloodSplatter>().Activate();
        }
    }
}
