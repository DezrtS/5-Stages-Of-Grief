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
        parentAttack.OnAttackTriggerEnter(other.transform);
    }

    //private void OnTriggerStay(Collider other)
    //{
    //    parentAttack.OnAttackTriggerStay(other.transform);
    //}

    private void OnTriggerExit(Collider other)
    {
        parentAttack.OnAttackTriggerExit(other.transform);
    }
}
