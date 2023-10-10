using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RangedAttack : Attack
{
    [Space(15)]
    [Header("Audio Variables")]
    [SerializeField] protected string playAudioIdOnAim;
    [SerializeField] protected string playAudioIdOnAttack;
    [SerializeField] protected string playAudioIdOnCancel;

    public override abstract float GetDamage();
}