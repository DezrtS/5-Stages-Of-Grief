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

    Vector3 AimDirection { get; }
    float RotationSpeed { get; }

    bool TransferToAttackState(AttackState attackState);

    void InitiateAttackState(AttackState attackState);

    bool CanInitiateAttackState(AttackState attackState);

    void OnAttackStateStart(AttackState attackState);

    void OnAttackState(AttackState attackState);

    void OnAttackStateEnd(AttackState attackState);

    void OnAttackStateCancel(AttackState attackState, bool otherHasCancelled);
}