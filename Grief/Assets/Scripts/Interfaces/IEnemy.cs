public enum EnemyState
{
    Spawning,
    Idle,
    Patrolling,
    Chasing,
    Repositioning,
    Attacking,
    Dodging,
    Fleeing,

    Stunned,
    Dying
}

public interface IEnemy 
{
    EnemyState EnemyState { get; }
    bool IsQueued { get; set; }
    bool HasRequested { get; set; }

    public void TransferToEnemyState(EnemyState enemyState);
    public void InitiateEnemyState(EnemyState enemyState);
    public bool CanInitiateEnemyState(EnemyState enemyState);
    public void OnEnemyStateStart(EnemyState enemyState);
    public void OnEnemyState(EnemyState enemyState);
    public void OnEnemyStateEnd(EnemyState enemyState);
}