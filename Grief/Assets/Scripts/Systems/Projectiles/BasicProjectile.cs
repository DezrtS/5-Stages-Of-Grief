using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicProjectile : MonoBehaviour
{
    protected Projectile projectileData;
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

    public void SetProjectileData(Projectile projectileData)
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
            OnProjectileHit(other);
        }
    }

    public virtual void OnProjectileHit(Collider other)
    {
        if (other.TryGetComponent(out IHealth entityHealth) && canDamage)
        {
            if (parentAttack == null)
            {
                DestroyProjectile();
            }
            else
            {
                if (parentAttack.OnAttackTriggerEnter(entityHealth, other.transform))
                {
                    DestroyProjectile();
                }
            }
        }

        if (other.tag == "Wall")
        {
            DestroyProjectile();
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