using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CombatManager : Singleton<CombatManager>
{
    public LayerMask attackLayer;
    public GameObject circleTrigger;

    public void DamageEntity<T>(T enitty) where T : IHealth
    {

    }

    public void InstantiateAttack(Vector3 position, float radius)
    {
        RaycastHit[] hits = Physics.SphereCastAll(position, radius, Vector3.up, Mathf.Infinity, attackLayer, QueryTriggerInteraction.Ignore);

        //foreach (RaycastHit hit in hits)
        //{
        //    if (hit.collider.tag == "Enemy")
        //    {
        //        
        //    }
        //}
    }

    public GameObject CreateCircleTrigger(Vector3 position, float scale)
    {
        GameObject trigger = Instantiate(circleTrigger, position, Quaternion.identity);
        trigger.transform.localScale = Vector3.one * scale;
        return trigger;
    }
}