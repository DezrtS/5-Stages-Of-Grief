using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : Singleton<CombatManager>
{
    public LayerMask bumpLayer;
    public GameObject circleAttackTrigger;

    public GameObject CreateCircleAttackTrigger(Attack attack, Vector3 position, float scale)
    {
        GameObject trigger = Instantiate(circleAttackTrigger, position, Quaternion.identity);
        trigger.transform.localScale = Vector3.one * scale;
        trigger.GetComponent<AttackTrigger>().SetParentAttack(attack);
        return trigger;
    }
}