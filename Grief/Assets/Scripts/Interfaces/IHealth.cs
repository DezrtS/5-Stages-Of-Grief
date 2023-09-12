public enum EntityType
{
    Player,
    Enemy,
    Boss,
    Object,
    Projectile
}

public interface IHealth
{
    EntityType EntityType { get; }
    float MaxHealth { get; }
    float Health { get; }

    bool IsInvincible { get; }
    void Damage(float damage);
    void Heal(float healing);
    void Die();
}