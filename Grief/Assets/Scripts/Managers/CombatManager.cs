using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : Singleton<CombatManager>
{
    public LayerMask attackLayer;
    public GameObject circleTrigger;

    public GameObject CreateCircleTrigger(Attack attack, Vector3 position, float scale)
    {
        GameObject trigger = Instantiate(circleTrigger, position, Quaternion.identity);
        trigger.transform.localScale = Vector3.one * scale;
        trigger.GetComponent<AttackTrigger>().SetParentAttack(attack);
        return trigger;
    }
}