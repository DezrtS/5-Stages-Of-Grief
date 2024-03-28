using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodDrop : MonoBehaviour
{
    DecalManager instance;

    private ParticleSystem part;
    private List<ParticleCollisionEvent> collisionEvents;

    void Start()
    {
        instance = DecalManager.Instance;
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);
        int i = 0;

        while (i < numCollisionEvents)
        {
            Vector3 pos = collisionEvents[i].intersection;
            instance.CreateBloodDecal(pos);
            i++;
        }
    }
}
