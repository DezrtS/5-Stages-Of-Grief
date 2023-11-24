using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashEffect : MonoBehaviour
{
    private FlashEffectData flashEffectData;

    private Material flashMaterial;
    private float flashTimer = 0;
    private float flashColorDuration = 0;
    private int flashColorIndex = 0;

    private Renderer objectRenderer;
    private Material[] originalMaterials;

    private bool flashStarted;

    public void Activate(FlashEffectData flashEffectData)
    {
        if (flashEffectData == null)
        {
            Deactivate();
            return;
        }

        this.flashEffectData = flashEffectData;

        objectRenderer = GetComponent<Renderer>();
        originalMaterials = objectRenderer.materials;

        flashMaterial = new Material(EffectManager.Instance.flashMaterial)
        {
            hideFlags = HideFlags.HideAndDontSave
        };

        Material[] newMaterials = new Material[originalMaterials.Length + 1];

        for (int i = 0; i < originalMaterials.Length; i++)
        {
            newMaterials[i] = originalMaterials[i];
        }

        newMaterials[originalMaterials.Length] = flashMaterial;

        objectRenderer.materials = newMaterials;

        flashStarted = true;
        flashColorDuration = flashEffectData.FlashColors[flashColorIndex].ColorDuration;
        flashMaterial.SetColor("_FlashColor", flashEffectData.FlashColors[flashColorIndex].Color);
        flashMaterial.SetFloat("_FlashAmount", flashEffectData.FlashColors[flashColorIndex].FlashAmount);
        flashMaterial.SetFloat("_Emission", flashEffectData.FlashColors[flashColorIndex].FlashEmission);
    }

    void Update()
    {
        if (flashStarted)
        {
            flashTimer += Time.deltaTime;

            if (flashTimer > flashColorDuration)
            {
                flashColorIndex++;

                if (flashColorIndex >= flashEffectData.FlashColors.Length)
                {
                    Debug.Log("Flash Deactivating");
                    Deactivate();
                    return;
                }

                flashColorDuration = flashEffectData.FlashColors[flashColorIndex].ColorDuration;
                flashMaterial.SetColor("_FlashColor", flashEffectData.FlashColors[flashColorIndex].Color);
                flashMaterial.SetFloat("_FlashAmount", flashEffectData.FlashColors[flashColorIndex].FlashAmount);
                flashMaterial.SetFloat("_Emission", flashEffectData.FlashColors[flashColorIndex].FlashEmission);
                flashTimer = 0;
            }
        }
    }

    public void Deactivate()
    {
        if (flashStarted)
        {
            objectRenderer.materials = originalMaterials;
        }

        Destroy(this);
    }

    void OnDestroy()
    {
        Destroy(flashMaterial);
    }
}