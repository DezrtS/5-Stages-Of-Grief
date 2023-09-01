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

    // Consider making this method static
    public void DamageEntity(Attack attack, IAttack attacker, IHealth reciever)
    {
        if (attacker.DamageTypes.Contains(reciever.HealthType))
        {
            reciever.Damage(attack.Damage);
        }
    }

    public void ApplyKnockback(Attack attack, Transform attacker, Transform reciever)
    {
        Vector3 knockback = (reciever.position - attacker.position).normalized * attack.KnockbackPower;
        knockback = new Vector3(knockback.x, 0, knockback.z);
        if (reciever.TryGetComponent(out RigidTransform rigidTransform))
        {
            rigidTransform.Velocity += knockback;
        } 
        else if (reciever.TryGetComponent(out Rigidbody rigidbody))
        {
            rigidbody.AddForce(knockback, ForceMode.Impulse);
        }
    }
}