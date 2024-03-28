using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodSplatter : MonoBehaviour
{
    [SerializeField] private Transform LongBlood;
    [SerializeField] private Transform FatBlood;
    [SerializeField] private Transform RingBlood;

    [SerializeField] private ParticleSystem bloodDrop;

    [SerializeField] private float bloodMoveSpeed = 0.1f;
    [SerializeField] private float effectDuration;
    private float timer;

    private Material bloodMaterial;

    public void Activate()
    {
        timer = effectDuration;
        bloodMaterial = RingBlood.GetComponentInChildren<MeshRenderer>().sharedMaterial;
        bloodDrop.Play();
    }

    private void Update()
    {
        if (timer > 0)
        {
            timer = Mathf.Max(timer - Time.deltaTime, 0);

            bloodMaterial.SetFloat("_Slice_Amount", 0.25f + 0.75f * (1f - (timer / effectDuration)));
            LongBlood.localPosition += new Vector3(0, 0, bloodMoveSpeed * Time.deltaTime);
            FatBlood.localPosition += new Vector3(0, 0, bloodMoveSpeed * Time.deltaTime);
            LongBlood.localScale += bloodMoveSpeed * Time.deltaTime * Vector3.one;
            FatBlood.localScale += bloodMoveSpeed * Time.deltaTime * Vector3.one;
            RingBlood.localScale += bloodMoveSpeed * Time.deltaTime * Vector3.one;

            if (timer <= 0)
            {
                timer = 0;
            }
        }
    }
}
