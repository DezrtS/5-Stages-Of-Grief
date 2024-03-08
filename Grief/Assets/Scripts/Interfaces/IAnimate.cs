using UnityEngine;

public enum AnimationEvent
{
    Aiming,
    Charging,
    Activating,
    Cooling,
    Canceling,
    Hurt,
    Die,
}

public interface IAnimate
{
    public Animator Animator { get; }
    public bool CanAnimate { get; }
    public void TriggerAnimation(string animationId);
    public void OnAnimationEventStart(AnimationEvent animationEvent, string animationId);
}