using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantDamageProjectile : BasicProjectile
{
    [SerializeField] private float delayBetweenDamage = 0.5f;
    private float timeSinceLastDamage = 0;

    private List<IHealth> entities = new List<IHealth>();

    private void OnTriggerEnter(Collider other)
    {
        if (TryGetComponent(out IHealth entity))
        {
            entities.Add(entity);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (TryGetComponent(out IHealth entity))
        {
            if (entities.Contains(entity))
            {
                entities.Remove(entity);
            }
        }
    }

    private void FixedUpdate()
    {
        if (Time.timeSinceLevelLoad - timeSinceLastDamage > delayBetweenDamage)
        {
            timeSinceLastDamage = Time.timeSinceLevelLoad;
            foreach (IHealth entity in entities)
            {
                parentAttack.DamageEntity(entity);
            }
        }    
    }

    public override void OnProjectileHit(Collider other)
    {

    }
}
