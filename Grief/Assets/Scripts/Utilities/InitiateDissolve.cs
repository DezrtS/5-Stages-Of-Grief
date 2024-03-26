using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitiateDissolve : MonoBehaviour
{
    //[SerializeField] private float animatorDelay;
    [SerializeField] private bool doOnlyRise = false;
    [SerializeField] private bool doDissolve = true;
    [SerializeField] private float delay;
    [SerializeField] private GameObject obj;
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem pSystem;

    //private bool animate;

    float timer = 0;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= delay)
        {
            if (!doOnlyRise)
            {
                obj.SetActive(true);
                pSystem.Play();
                if (doDissolve)
                {
                    EffectManager.Instance.Dissolve(transform, true);
                }
            }
            animator.SetTrigger("Rise");
            Destroy(this);
        }
    }
}
