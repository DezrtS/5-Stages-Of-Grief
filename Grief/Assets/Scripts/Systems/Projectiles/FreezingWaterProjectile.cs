using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezingWaterProjectile : BasicProjectile
{
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private GameObject testPlatform;
    private bool hitWater = false;
    Vector3 startHitPoint;
    GameObject platform;

    public override void OnProjectile()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 5f, hitMask, QueryTriggerInteraction.Ignore))
        {
            if (hitWater)
            {
                Vector3 direction = hit.point - startHitPoint;
                Vector3 midPoint = startHitPoint + direction.normalized * (direction.magnitude / 2f);
                platform.transform.position = midPoint + Vector3.down * 0.5f;
                platform.transform.localScale = new Vector3(5, 1, direction.magnitude);
            }
            else
            {
                startHitPoint = hit.point;
                platform = Instantiate(testPlatform, startHitPoint, Quaternion.identity);
                platform.transform.forward = transform.forward;
                platform.transform.localScale = new Vector3(5, 1, 3);
                hitWater = true;
            }
        }

        base.OnProjectile();
    }
}