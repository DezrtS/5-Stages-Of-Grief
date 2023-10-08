using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour
{
    [SerializeField] private Color activationColor;
    [SerializeField] private Color deactivationColor;

    [Space(10)]
    [SerializeField] private Image iceShardImage;
    [SerializeField] private Image fireBlastImage;
    [SerializeField] private Image teleportImage;

    private int selectedAbility = 0;

    private void Start()
    {
        PlayerController.Instance.OnAbilitySelectEvent += SelectAbility;
        DeactivateAll();
        Activate(iceShardImage);
    }

    public void SelectAbility(int ability)
    {
        if (selectedAbility == ability)
        {
            return;
        }

        if (ActivateAndDeactivate(GetAbilityImage(ability), GetAbilityImage(selectedAbility)))
        {
            selectedAbility = ability;
        }
    }

    public Image GetAbilityImage(int ability)
    {
        switch (ability)
        {
            case 0:
                return iceShardImage;
            case 1:
                return fireBlastImage;
            case 2:
                return teleportImage;
            default:
                return null;
        }
    }

    public void Activate(Image activatedImage)
    {
        activatedImage.color = activationColor;
    }

    public bool ActivateAndDeactivate(Image activatedImage, Image deactivatedImage)
    {
        if (activatedImage == null || deactivatedImage == null)
        {
            return false;
        }

        deactivatedImage.color = deactivationColor;
        activatedImage.color = activationColor;

        return true;
    }

    public void DeactivateAll()
    {
        iceShardImage.color = deactivationColor;
        fireBlastImage.color = deactivationColor;
        teleportImage.color = deactivationColor;
    }

    private void OnDisable()
    {
        PlayerController.Instance.OnAbilitySelectEvent -= SelectAbility;
    }
}
