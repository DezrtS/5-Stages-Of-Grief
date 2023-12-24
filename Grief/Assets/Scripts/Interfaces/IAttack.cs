using System.Collections.Generic;

public enum AttackState
{
    Idle,
    Aiming,
    ChargingUp,
    Attacking,
    CoolingDown
}

public interface IAttack
{
    AttackHolder AttackHolder { get; }
    bool IsAttacking { get; }
    AttackState AttackState { get; }
    List<EntityType> DamageableEntities { get; }

    void TransferToAttackState(AttackState attackState);
    void InitiateAttackState(AttackState attackState);
    bool CanInitiateAttackState(AttackState attackState, string attackId);
    void OnAttackStateStart(AttackState attackState);
    void OnAttackState(AttackState attackState);
    void OnAttackStateEnd(AttackState attackState);
    void OnAttackStateCancel(AttackState attackState, bool otherHasCancelled);
}