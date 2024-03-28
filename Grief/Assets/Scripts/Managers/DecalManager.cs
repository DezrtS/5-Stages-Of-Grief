using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalManager : Singleton<DecalManager>
{
    [SerializeField] private int maxBloodDecals = 50;
    [SerializeField] private GameObject bloodDecal;
    private GameObject[] bloodDecals;
    private int currentBloodDecal = 0;

    protected override void Awake()
    {
        base.Awake();
        bloodDecals = new GameObject[maxBloodDecals];
    }

    public void CreateBloodDecal(Vector3 position)
    {
        if (currentBloodDecal >= 50)
        {
            currentBloodDecal = 0;
        }

        if (bloodDecals[currentBloodDecal] != null)
        {
            Destroy(bloodDecals[currentBloodDecal]);
        }
        bloodDecals[currentBloodDecal] = Instantiate(bloodDecal, position, Quaternion.identity, transform);
        currentBloodDecal++;
    }
}