public enum AnimationEvent
{
    AimAttack,
    AimDodge,
    AimAttackCancel,
    AimDodgeCancel,
    Attack,
    Dodge,

    Walk,
    Run,
    Stand,

    Hurt,
    Die,
}

public interface IAnimate
{
    public void OnAnimationStart(AnimationEvent animationEvent, string animationId);
}