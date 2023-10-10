using System.Collections.Generic;
using UnityEngine;

public interface IStatusEffectTarget
{
    List<StatusEffect> StatusEffects { get; }

    void AddStatusEffect(StatusEffect statusEffect);
    void RemoveStatusEffect(StatusEffect statusEffect);
    void ClearStatusEffects();
}