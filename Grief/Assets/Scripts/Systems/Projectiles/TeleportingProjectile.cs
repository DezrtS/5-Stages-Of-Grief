using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportingProjectile : BasicProjectile
{
    public override void OnProjectileHit(Transform hit)
    {
        base.OnProjectileHit(hit);
            
        parentAttack.ParentTransform.GetComponent<MovementController>().Teleport(transform.position);
        DestroyProjectile();
    }
}
