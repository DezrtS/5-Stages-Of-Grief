using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMODEventsManager : Singleton<FMODEventsManager>
{
    [field: Header("Music")]
    [field: SerializeField] public EventReference DenialTheme { get; private set; }
    [field: SerializeField] public EventReference FrozenLakeTheme1 { get; private set; }

    [field: Header("Ambience")]
    [field: SerializeField] public EventReference Wind { get; private set; }

    [field: Header("SFX")]
    [field: Header("    Ambience")]
    [field: SerializeField] public EventReference CrowCaw { get; private set; }
    [field: SerializeField] public EventReference WolfSnarl { get; private set; }
    [field: Header("    Dialogue")]
    [field: SerializeField] public EventReference PlayerDialogue { get; private set; }
    [field: Header("    Player")]
    [field: SerializeField] public EventReference PlayerHurt { get; private set; }
    [field: Header("    Movement")]
    [field: SerializeField] public EventReference GeneralFootsteps { get; private set; }
    [field: SerializeField] public EventReference RockEnemyRolling { get; private set; }
    [field: Header("    Attacks")]
    [field: SerializeField] public EventReference PlayerSwordSwing { get; private set; }
    [field: SerializeField] public EventReference BossSwordSwing { get; private set; }
    [field: SerializeField] public EventReference ClawAttack { get; private set; }
    [field: SerializeField] public EventReference IceFreeze { get; private set; }
    [field: Header("    Hits")]
    [field: SerializeField] public EventReference GeneralHit { get; private set; }
    [field: SerializeField] public EventReference RollIntoTree { get; private set; }
    [field: SerializeField] public EventReference RollIntoRock { get; private set; }
    [field: SerializeField] public EventReference RollIntoIce { get; private set; }
    [field: Header("    Misc")]
    [field: SerializeField] public EventReference IceBreak { get; private set; }
}