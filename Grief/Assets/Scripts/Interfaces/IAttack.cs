using System.Collections.Generic;
using UnityEngine;

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
    Attack ActiveAttack { get; }
    bool IsAttacking { get; }
    AttackState AttackState { get; }
    List<EntityType> DamageableEntities { get; }

    void SwitchActiveAttack(Attack activeAttack);
    bool CanSwitchActiveAttack(Attack activeAttack);
    void TransferToAttackState(AttackState attackState);
    void InitiateAttackState(AttackState attackState);
    bool CanInitiateAttackState(AttackState attackState, string attackId);
    void OnAttackStateStart(AttackState attackState);
    void OnAttackState(AttackState attackState);
    void OnAttackStateEnd(AttackState attackState);
    void OnAttackStateCancel(AttackState attackState, bool otherHasCancelled);
}