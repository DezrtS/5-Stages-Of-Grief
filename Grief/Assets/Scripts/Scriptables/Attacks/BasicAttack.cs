using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Attacks/Physical Attacks/Basic Attack")]
public class BasicAttack : PhysicalAttack
{
    private GameObject attackTrigger;
    private Vector3 triggerOffset;

    public override void OnAttackStateStart(AttackState attackState)
    {
        base.OnAttackStateStart(attackState);

        if (attackState == AttackState.Idle)
        {
            PlayAudio(playAudioIdOnCooldown);
        }
        else if (attackState == AttackState.Aiming)
        {
            PlayAudio(playAudioIdOnAim);
        }
        else if (attackState == AttackState.Attacking)
        {
            attackTrigger = CombatManager.Instance.CreateCircleTrigger(this, parentTransform.position + parentTransform.forward * attackTriggerSpawnDistance, attackTriggerScale);
            triggerOffset = attackTrigger.transform.position - parentTransform.position;
            PlayAudio(playAudioIdOnAttack);
        }
    }

    public override void OnAttackState(AttackState attackState, float timeSinceStateStart)
    {
        if (attackState == AttackState.Attacking)
        {
            attackTrigger.transform.position = parentTransform.position + triggerOffset;
        }

        base.OnAttackState(attackState, timeSinceStateStart);
    }

    public override void OnAttackStateEnd(AttackState attackState)
    {
        base.OnAttackStateEnd(attackState);

        if (attackState == AttackState.Attacking)
        {
            Destroy(attackTrigger);
        }
    }

    public override void OnAttackStateCancel(AttackState attackState, bool otherHasCancelled)
    {
        base.OnAttackStateCancel(attackState, otherHasCancelled);

        if (attackState == AttackState.Attacking)
        {
            Destroy(attackTrigger);
            PlayAudio(playAudioIdOnCancel);
        }
    }
}