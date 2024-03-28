using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RandomScale : MonoBehaviour
{
    [SerializeField] private float minScale;
    [SerializeField] private float maxScale = 1;

    private void Awake()
    {
        DecalProjector decalProjector = GetComponent<DecalProjector>();
        Vector3 randomScale = Vector3.one * Random.Range(minScale, maxScale);
        randomScale.z = 1;
        decalProjector.size = randomScale; 
    }
}
