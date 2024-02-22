using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashEffect : MonoBehaviour, IEffect
{
    private EffectType type = EffectType.Damage;

    public EffectType Type { get { return type; } }

    public bool CanBeOverrided { get { return true; } }

    private FlashEffectData flashEffectData;

    private Material flashMaterial;
    private float flashTimer = 0;
    private float flashColorDuration = 0;
    private int flashColorIndex = 0;

    private Renderer objectRenderer;
    private Material[] originalMaterials;

    private bool flashStarted;

    public void ActivateEffect()
    {
        flashEffectData = EffectManager.Instance.flashEffectData;

        objectRenderer = GetComponent<Renderer>();
        originalMaterials = objectRenderer.sharedMaterials;

        flashMaterial = new Material(EffectManager.Instance.flashMaterial)
        {
            hideFlags = HideFlags.HideAndDontSave
        };

        Material[] newMaterials = new Material[originalMaterials.Length];

        for (int i = 0; i < originalMaterials.Length; i++)
        {
            newMaterials[i] = flashMaterial;
        }

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
                    //Debug.Log("Flash Deactivating");
                    DeactivateEffect();
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

    public void RestartEffect()
    {
        if (flashStarted)
        {
            flashTimer = 0;
            flashColorDuration = flashEffectData.FlashColors[flashColorIndex].ColorDuration;
            flashColorIndex = 0;

            flashMaterial.SetColor("_FlashColor", flashEffectData.FlashColors[flashColorIndex].Color);
            flashMaterial.SetFloat("_FlashAmount", flashEffectData.FlashColors[flashColorIndex].FlashAmount);
            flashMaterial.SetFloat("_Emission", flashEffectData.FlashColors[flashColorIndex].FlashEmission);
        } 
        else
        {
            ActivateEffect();
        }
    }

    public void DeactivateEffect()
    {
        if (flashStarted)
        {
            objectRenderer.materials = originalMaterials;
        }

        Destroy(this);
    }
}