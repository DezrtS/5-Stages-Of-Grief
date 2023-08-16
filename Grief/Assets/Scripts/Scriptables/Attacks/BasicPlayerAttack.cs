using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Attacks/Basic Player Attack")]
public class BasicPlayerAttack : Attack
{
    private GameObject attackTrigger;
    private Vector3 triggerOffset;

    public override void OnAttackStateStart<T>(AttackState attackState, T attacker)
    {
        base.OnAttackStateStart<T>(attackState, attacker);

        if (attackState == AttackState.Attacking)
        {
            attackTrigger = CombatManager.Instance.CreateCircleTrigger(parentTransform.position + parentTransform.forward * 2, 3);
            triggerOffset = attackTrigger.transform.position - parentTransform.position;
        }
    }

    public override void OnAttackState<T>(AttackState attackState, T attacker, float timeSinceStateStart)
    {
        base.OnAttackState(attackState, attacker, timeSinceStateStart);

        if (attackState == AttackState.Attacking)
        {
            attackTrigger.transform.position = parentTransform.position + triggerOffset;
        }
    }

    public override void OnAttackStateEnd<T>(AttackState attackState, T attacker)
    {
        base.OnAttackStateEnd(attackState, attacker);

        if (attackState == AttackState.Attacking)
        {
            Destroy(attackTrigger);
        }
    }

    public override void OnAttackStateCancel<T>(AttackState attackState, T attacker, bool otherHasCancelled)
    {
        base.OnAttackStateCancel(attackState, attacker, otherHasCancelled);

        if (attackState == AttackState.Attacking)
        {
            Destroy(attackTrigger);
        }
    }
}