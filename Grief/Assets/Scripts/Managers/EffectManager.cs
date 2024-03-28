using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EffectManager : Singleton<EffectManager>
{
    public FlashEffectData flashEffectData;

    public Material flashMaterial;
    public Material dissolveMaterial;

    public GameObject bloodSplatter;
    private GameObject activeSplatter = null;

    public void Dissolve(Transform dissolveObject, bool dissolveIn)
    {
        Renderer[] renderers = dissolveObject.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            if (renderer.CompareTag("Uneffectable"))
            {
                continue;
            }

            if (renderer.transform.TryGetComponent(out IEffect effect))
            {
                if (dissolveIn)
                {
                    if (effect.Type == EffectType.Spawn)
                    {
                        effect.RestartEffect();
                        continue;
                    }
                } 
                else
                {
                    if (effect.Type == EffectType.Death)
                    {
                        effect.RestartEffect();
                        continue;
                    }
                }

                if (effect.CanBeOverrided)
                {
                    effect.DeactivateEffect();
                }
                else 
                {
                    continue;
                }
            }

            DissolveEffect newDissolve = renderer.transform.AddComponent<DissolveEffect>();
            newDissolve.SetDissolveIn(dissolveIn);
            newDissolve.ActivateEffect();
        }
    }

    public void Flash(Transform flashObject)
    {
        Renderer[] renderers = flashObject.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            if (renderer.CompareTag("Uneffectable"))
            {
                continue;
            }

            if (renderer.transform.TryGetComponent(out IEffect effect))
            {
                if (effect.Type == EffectType.Damage)
                {
                    effect.RestartEffect();
                    continue;
                }
                
                if (effect.CanBeOverrided)
                {
                    effect.DeactivateEffect();
                }
                else
                {
                    continue;
                }
            }

            FlashEffect newFlash = renderer.transform.AddComponent<FlashEffect>();
            newFlash.ActivateEffect();
        }
    }

    public void Bleed(Transform bleedObject, Vector3 direction)
    {
        if (activeSplatter != null)
        {
            Destroy(activeSplatter);
        }

        activeSplatter = Instantiate(bloodSplatter, bleedObject.transform.position + Vector3.up * 2 + direction * 2, Quaternion.identity, transform);
        activeSplatter.transform.forward = direction;
        activeSplatter.GetComponent<BloodSplatter>().Activate();
    }
}