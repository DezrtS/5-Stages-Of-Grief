using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;

    private bool isWalking;

    public void Swing()
    {
        playerAnimator.SetTrigger("Swing");
    }

    public void Dodge()
    {
        playerAnimator.SetTrigger("Dodge");
    }

    public void SetWalking(bool isWalking)
    {
        if (this.isWalking == isWalking)
        {
            return;
        }

        this.isWalking = isWalking;
        playerAnimator.SetBool("IsWalking", isWalking);
    }
}
