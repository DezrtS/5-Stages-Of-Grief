using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : Singleton<EffectManager>
{
    [SerializeField] private GameObject flashObject;
    [SerializeField] private FlashEffectData flashEffectData;

    public Material flashMaterial;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            if (TryGetComponent(out FlashEffect oldFlash))
            {
                oldFlash.Deactivate();
            }

            FlashEffect newFlash = flashObject.AddComponent<FlashEffect>();
            newFlash.Activate(flashEffectData);
        }
    }
}