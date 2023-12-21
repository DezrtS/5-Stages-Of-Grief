public interface IStatusEffectTarget
{
    StatusEffectHolder StatusEffectHolder { get; }

    public void ApplyHealthStatusEffect(float healthEffectValue, bool isDamage);

    // Stun, Movement Related, Etc.
}