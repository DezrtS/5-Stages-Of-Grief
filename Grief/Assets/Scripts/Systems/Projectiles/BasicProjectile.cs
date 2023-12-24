using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicProjectile : MonoBehaviour
{
    protected ProjectileData projectileData;
    protected Attack parentAttack;
    protected bool canDamage = true;
    protected bool isBeingDestroyed = false;
    private bool isFired;

    private float spawnTime = 0;

    private void Start()
    {
        spawnTime = Time.timeSinceLevelLoad;
    }

    private void FixedUpdate()
    {
        if (Time.timeSinceLevelLoad - spawnTime >= projectileData.ProjectileLifespan && !isBeingDestroyed)
        {
            DestroyProjectile();
        }
    }

    public void SetProjectileData(ProjectileData projectileData)
    {
        this.projectileData = projectileData;
    }

    public void SetParentAttack(Attack parentAttack)
    {
        this.parentAttack = parentAttack;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isFired)
        {
            OnProjectileHit(other.transform);
        }
    }

    public virtual void OnProjectileHit(Transform hit)
    {
        if (hit.CompareTag("Wall") || parentAttack == null)
        {
            DestroyProjectile();
            return;
        }

        if (canDamage)
        {
            parentAttack.OnProjectileHit(this, hit, projectileData.Damage, projectileData.KnockbackPower);
        }
    }

    public virtual void OnFireProjectile()
    {
        isFired = true;
    }

    public virtual void DestroyProjectile()
    {
        canDamage = false;
        isBeingDestroyed = true;
        Destroy(gameObject);
    }
}