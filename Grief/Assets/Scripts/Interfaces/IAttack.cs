public interface IAttack
{
    Attack Attack { get; }
    bool IsAttacking { get; }

    void InitiateAttack();

    bool CanAttack();

    void OnAttackStart();

    void OnAttackCancel();

    void OnAttackEnd();
}