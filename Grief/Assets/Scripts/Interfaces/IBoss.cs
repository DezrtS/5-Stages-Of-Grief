public enum BossState
{
    Idle,
    Patrolling,
    Chasing,
    Repositioning,
    Attacking,
    Dodging,
    Fleeing,

    Stunned,
    Dead
}

public interface IBoss
{
    BossState BossState { get; }

    public void TransferToBossState(BossState enemyState);
    public void InitiateBossState(BossState enemyState);
    public bool CanInitiateBossState(BossState enemyState);
    public void OnBossStateStart(BossState enemyState);
    public void OnBossState(BossState enemyState);
    public void OnBossStateEnd(BossState enemyState);
}