using UnityEngine;

public interface IStatusEffectTarget
{
    StatusEffectHolder StatusEffectHolder { get; }
    GameObject ParticleEffectHolder { get; }
}