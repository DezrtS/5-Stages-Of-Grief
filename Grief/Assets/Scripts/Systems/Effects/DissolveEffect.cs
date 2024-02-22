using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveEffect : MonoBehaviour, IEffect
{
    private EffectType type;
    private bool dissolveIn = true;

    public EffectType Type { get { return type; } }

    public bool CanBeOverrided { get { return false; } }

    private Material dissolveMaterial;
    private float effectTimer = 0;
    private float effectDuration = 2;

    private Renderer objectRenderer;
    private Material[] originalMaterials;

    private bool dissolveStarted;

    public void ActivateEffect()
    {
        objectRenderer = GetComponent<Renderer>();
        originalMaterials = objectRenderer.sharedMaterials;

        dissolveMaterial = new Material(EffectManager.Instance.dissolveMaterial)
        {
            hideFlags = HideFlags.HideAndDontSave
        };

        dissolveMaterial.SetFloat("_DissolveStrength", 1);

        Material[] newMaterials = new Material[originalMaterials.Length];

        for (int i = 0; i < originalMaterials.Length; i++)
        {
            newMaterials[i] = dissolveMaterial;
        }

        objectRenderer.materials = newMaterials;

        dissolveStarted = true;
    }

    void FixedUpdate()
    {
        if (dissolveStarted)
        {
            effectTimer += Time.fixedDeltaTime;

            if (effectTimer < effectDuration)
            {
                if (dissolveIn)
                {
                    dissolveMaterial.SetFloat("_DissolveStrength", 1 - (effectTimer / effectDuration));
                }
                else
                {
                    dissolveMaterial.SetFloat("_DissolveStrength", effectTimer / effectDuration);
                }
            }
            else
            {
                DeactivateEffect();
            }
        }
    }

    public void RestartEffect()
    {
        if (dissolveStarted)
        {
            effectTimer = 0;
        }
        else
        {
            ActivateEffect();
        }
    }

    public void DeactivateEffect()
    {
        if (dissolveStarted)
        {
            objectRenderer.materials = originalMaterials;
        }

        Destroy(this);
    }

    public void SetDissolveIn(bool dissolveIn)
    {
        this.dissolveIn = dissolveIn;

        if (dissolveIn)
        {
            type = EffectType.Spawn;
        }
        else
        {
            type = EffectType.Death;
        }
    }
}