using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Attacks/Player Basic Attack")]
public class PlayerAttack : Attack
{
    private GameObject attackTrigger;
    private Vector3 triggerOffset;

    public override void OnAimToAttack(Transform transform)
    {
        base.OnAimToAttack(transform);
        attackTrigger = CombatManager.Instance.CreateCircleTrigger(transform.position + transform.forward * 2, 3);
        triggerOffset = attackTrigger.transform.position - transform.position;
    }

    public override void OnAttack(Transform transform)
    {
        attackTrigger.transform.position = transform.position + triggerOffset;
    }

    public override void OnAttackEnd(Transform transform)
    {
        base.OnAttackEnd(transform);
        Destroy(attackTrigger);
    }
}