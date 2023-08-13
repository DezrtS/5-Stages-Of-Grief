using UnityEngine;

public enum AttackState
{
    Idle,
    Aiming,
    Attacking
}

public interface IAttack
{
    Attack Attack { get; }

    AttackState AttackState { get; }

    Vector3 AimDirection { get; }
    float RotationSpeed { get; }

    bool TransferToState(AttackState attackState);

    void InitiateState(AttackState attackState);

    bool CanInitiateState(AttackState attackState);

    void OnStateStart(AttackState attackState);

    void OnState(AttackState attackState);

    void OnStateEnd(AttackState attackState);

    void OnStateCancel(AttackState attackState);
}