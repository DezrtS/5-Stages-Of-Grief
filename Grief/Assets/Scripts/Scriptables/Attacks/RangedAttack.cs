using FMODUnity;
using UnityEngine;

public abstract class RangedAttack : Attack
{
    [Space(15)]
    [Header("Audio Variables")]
    [field: SerializeField] protected EventReference playAudioIdOnAim;
    [field: SerializeField] protected EventReference playAudioIdOnAttack;
    [field: SerializeField] protected EventReference playAudioIdOnCancel;

    [Space(15)]
    [Header("Particle Effect Variables")]
    [SerializeField] protected GameObject particleEffectOnAimPrefab;
    [SerializeField] protected GameObject particleEffectOnAttackPrefab;
    [SerializeField] protected GameObject particleEffectOnCancelPrefab;

    protected ParticleSystem particleEffectOnAim;
    protected ParticleSystem particleEffectOnAttack;
    protected ParticleSystem particleEffectOnCancel;
}