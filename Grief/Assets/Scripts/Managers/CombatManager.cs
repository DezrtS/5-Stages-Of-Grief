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
    public bool DamageEntity(Attack attack, IAttack attacker, IHealth reciever, Transform attackerTransform, Transform recieverTransform)
    {

        if (attacker.DamageableEntities.Contains(reciever.EntityType))
        {
            ApplyKnockback(attack, attackerTransform, recieverTransform);

            reciever.Damage(attack.GetDamage());
            //EffectManager.Instance.Flash(recieverTransform);

            return true;
        }

        return false;
    }

    public bool DamageEntity(Attack attack, IAttack attacker, IHealth reciever)
    {

        if (attacker.DamageableEntities.Contains(reciever.EntityType))
        {
            reciever.Damage(attack.GetDamage());

            return true;
        }

        return false;
    }

    public void ApplyKnockback(Attack attack, Transform attacker, Transform reciever)
    {
        Vector3 knockback = (reciever.position - attacker.position).normalized * 3; //attack.KnockbackPower;
        knockback = new Vector3(knockback.x, 0, knockback.z);
        if (reciever.TryGetComponent(out MovementController movementController))
        {
            movementController.ApplyForce(knockback);
        } 
        else if (reciever.TryGetComponent(out Rigidbody rigidbody))
        {
            rigidbody.AddForce(knockback, ForceMode.Impulse);
        }
    }
}