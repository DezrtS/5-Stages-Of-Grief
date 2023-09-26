using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;

    public void Swing()
    {
        playerAnimator.SetTrigger("Swing");
    }

    public void Dodge()
    {
        playerAnimator.SetTrigger("Dodge");
    }
}
