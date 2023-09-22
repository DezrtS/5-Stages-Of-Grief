public enum EnemyState
{
    Deciding,
    Idle,
    Patrolling,
    Chasing,
    Attacking,
    Fleeing,
    Stunned,
    Dead
}

public interface IEnemy 
{
    EnemyState EnemyState { get; }

    public void TransferToEnemyState(EnemyState enemyState);
    public void InitiateEnemyState(EnemyState enemyState);
    public bool CanInitiateEnemyState(EnemyState enemyState);
    public void OnEnemyStateStart(EnemyState enemyState);
    public void OnEnemyState(EnemyState enemyState);
    public void OnEnemyStateEnd(EnemyState enemyState);
    public void OnIdle();
    public void OnPatrolling();
    public void OnChasing();
    public void OnAttacking();
    public void OnFleeing();
    public void OnStunned();
    public void OnDead();
}