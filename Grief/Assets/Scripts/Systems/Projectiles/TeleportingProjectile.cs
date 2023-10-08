using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportingProjectile : BasicProjectile
{
    public override void OnProjectileHit(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            parentAttack.ParentTransform.GetComponent<MovementController>().Teleport(transform.position);
            DestroyProjectile();
        }
    }
}
