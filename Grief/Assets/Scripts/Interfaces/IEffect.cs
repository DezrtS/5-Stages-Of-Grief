public enum EffectType
{
    Default,
    Damage,
    Spawn,
    Death
}

public interface IEffect
{
    EffectType Type { get; }
    bool CanBeOverrided { get; }
    void ActivateEffect();
    void RestartEffect();
    void DeactivateEffect();
}