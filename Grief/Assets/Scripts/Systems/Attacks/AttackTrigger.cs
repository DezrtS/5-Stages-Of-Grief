using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTrigger : MonoBehaviour
{
    private Attack parentAttack;

    public void SetParentAttack(Attack attack)
    {
        parentAttack = attack;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IHealth entityHealth))
        {
            parentAttack.OnAttackTriggerEnter(entityHealth, other.transform);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out IHealth entityHealth))
        {
            parentAttack.OnAttackTriggerStay(entityHealth);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IHealth entityHealth))
        {
            parentAttack.OnAttackTriggerExit(entityHealth);
        }
    }
}
