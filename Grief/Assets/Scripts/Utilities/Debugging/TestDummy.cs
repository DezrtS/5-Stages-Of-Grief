using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TestDummy : Enemy
{
    public override void OnEnemyStateStart(EnemyState enemyState)
    {
        if (enemyState == EnemyState.Attacking)
        {
            AudioManager.Instance.PlayOneShot(FMODEventsManager.Instance.WolfSnarl, transform.position);
        }
        base.OnEnemyStateStart(enemyState);
    }
}