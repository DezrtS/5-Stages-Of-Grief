public enum HealthType
{
    Player,
    Enemy,
    Object,
    Projectile
}

public interface IHealth
{
    HealthType HealthType { get; }
    float MaxHealth { get; }
    float Health { get; }

    bool IsInvincible { get; }

    void Damage(float damage);

    void Heal(float healing);

    void Die();
}