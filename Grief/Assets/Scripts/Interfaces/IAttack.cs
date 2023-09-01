using System.Collections.Generic;
using UnityEngine;

public enum AttackState
{
    Idle,
    Aiming,
    Attacking,
    CoolingDown
}

public interface IAttack
{
    Attack Attack { get; }

    AttackState AttackState { get; }

    List<HealthType> DamageTypes { get; }

    float RotationSpeed { get; }

    void TransferToAttackState(AttackState attackState);

    void InitiateAttackState(AttackState attackState);

    bool CanInitiateAttackState(AttackState attackState, string attackId);

    void OnAttackStateStart(AttackState attackState);

    void OnAttackState(AttackState attackState);

    void OnAttackStateEnd(AttackState attackState);

    void OnAttackStateCancel(AttackState attackState, bool otherHasCancelled);
}