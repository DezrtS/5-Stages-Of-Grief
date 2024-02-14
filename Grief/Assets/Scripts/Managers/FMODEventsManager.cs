using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMODEventsManager : Singleton<FMODEventsManager>
{
    [field: Header("Music")]
    [field: SerializeField] public EventReference DenialTheme1 { get; private set; }
    [field: SerializeField] public EventReference FrozenLakeTheme1 { get; private set; }

    [field: Header("Ambience")]
    [field: SerializeField] public EventReference ambience { get; private set; }
    [field: SerializeField] public EventReference crow { get; private set; }

    [field: Header("General SFX")]
    [field: SerializeField] public EventReference hit { get; private set; }
    [field: SerializeField] public EventReference iceBreak { get; private set; }
    [field: SerializeField] public EventReference footsteps { get; private set; }
    [field: SerializeField] public EventReference snarl { get; private set; }
    [field: SerializeField] public EventReference dialogue { get; private set; }

    [field: Header("Player SFX")]
    [field: SerializeField] public EventReference playerHurt { get; private set; }
    [field: SerializeField] public EventReference playerFootsteps { get; private set; }
    [field: SerializeField] public EventReference playerSwing { get; private set; }
}