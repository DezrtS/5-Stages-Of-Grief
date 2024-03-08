using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBox : MonoBehaviour, IHealth
{
    [Header("Health")]
    private readonly EntityType entityType = EntityType.Object;
    [SerializeField] private float maxHealth;
    private float health;
    private bool isInvincible;

    public EntityType EntityType { get { return entityType; } }
    public float MaxHealth { get { return maxHealth; } }
    public float Health { get { return health; } }
    public bool IsInvincible { get { return isInvincible; } }

    private void Start()
    {
        health = maxHealth;
    }

    public void Damage(float damage)
    {
        if (isInvincible)
        {
            return;
        }

        EffectManager.Instance.Flash(transform);

        health = Mathf.Max(health - damage, 0);

        //Debug.Log($"Test Box has lost {damage} health at {Time.timeSinceLevelLoad} seconds since level load");

        if (health == 0)
        {
            Die();
        }
    }

    public void Heal(float healing)
    {
        health = Mathf.Min(health + healing, maxHealth);
    }

    public void Die()
    {
        Destroy(gameObject);
        Debug.Log("Test Box Has Died");
    }
}
