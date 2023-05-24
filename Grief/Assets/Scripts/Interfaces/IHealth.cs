public interface IHealth
{
    float MaxHealth { get; }
    float Health { get; }

    void Damage(float damage);

    void Heal(float healing);

    void Die();
}