using System.Collections.Generic;

public interface IStatusEffectTarget
{
    List<StatusEffect> StatusEffects { get; }
    bool IsStunned { get; }

    void AddStatusEffect(StatusEffect statusEffect);
    void RemoveStatusEffect(StatusEffect statusEffect);
    void ClearStatusEffects();

    void Stun(bool isStunned);
}