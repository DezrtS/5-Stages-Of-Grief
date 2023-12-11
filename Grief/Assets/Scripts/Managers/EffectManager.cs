using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EffectManager : Singleton<EffectManager>
{
    [SerializeField] private GameObject flashObject;
    [SerializeField] private FlashEffectData flashEffectData;

    public Material flashMaterial;

    /*
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
    */

    public void Flash(Transform flashObject)
    {
        Renderer[] renderers = flashObject.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            if (renderer.CompareTag("Unflashable"))
            {
                continue;
            }

            if (renderer.transform.TryGetComponent(out FlashEffect oldFlash))
            {
                oldFlash.Restart(flashEffectData);
                continue;
            }

            FlashEffect newFlash = renderer.transform.AddComponent<FlashEffect>();
            newFlash.Activate(flashEffectData);
        }
    }
}