using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Attacks/Player Basic Attack")]
public class PlayerAttack : Attack
{
    private GameObject attackTrigger;
    private Vector3 triggerOffset;

    public override void OnStateStart<T>(AttackState attackState, T attacker)
    {
        base.OnStateStart<T>(attackState, attacker);

        if (attackState == AttackState.Attacking)
        {
            attackTrigger = CombatManager.Instance.CreateCircleTrigger(parentTransform.position + parentTransform.forward * 2, 3);
            triggerOffset = attackTrigger.transform.position - parentTransform.position;
        }
    }

    public override void OnState<T>(AttackState attackState, T attacker, float timeSinceStateStart)
    {
        base.OnState(attackState, attacker, timeSinceStateStart);

        if (attackState == AttackState.Attacking)
        {
            attackTrigger.transform.position = parentTransform.position + triggerOffset;
        }
    }

    public override void OnStateEnd<T>(AttackState attackState, T attacker)
    {
        base.OnStateEnd(attackState, attacker);

        if (attackState == AttackState.Attacking)
        {
            Destroy(attackTrigger);
        }
    }
}