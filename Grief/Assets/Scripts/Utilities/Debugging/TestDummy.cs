using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDummy : MonoBehaviour, IHealth, IMove, IPathfind
{
    [Header("Health")]
    private readonly HealthType healthType = HealthType.Enemy;
    [SerializeField] private float maxHealth;
    private float health;
    private bool isInvincible;

    private Vector3 pathfindPosition;

    private RigidTransform rigidTrans;

    private float rotationSpeed;

    private bool isPathfinding;
    private Vector3 pathfindDestination;

    public HealthType HealthType { get { return healthType; } }
    public float MaxHealth { get { return maxHealth; } }
    public float Health { get { return health; } }
    public bool IsInvincible { get { return isInvincible; } }

    public float RotationSpeed { get { return rotationSpeed; } }
    public bool IsPathfinding { get { return isPathfinding; } }
    public Vector3 PathfindDestination { get { return pathfindDestination; } set { pathfindDestination = value; } }

    private void Start()
    {

        rigidTrans = GetComponent<RigidTransform>();
        health = maxHealth;
    }

    public void Damage(float damage)
    {
        if (isInvincible)
        {
            return;
        }

        health = Mathf.Max(health - damage, 0);

        Debug.Log($"Test dummy has lost {damage} health at {Time.timeSinceLevelLoad} seconds since level load");

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
        Debug.Log("Test Dummy Has Died");
    }

    public Vector3 GetMovementInput()
    {
        return Vector3.zero;
    }

    public Vector3 GetRotationInput()
    {
        return GetMovementInput();
    }

    public void TransferToPathfindingState(bool isPathfinding)
    {
        this.isPathfinding = isPathfinding;
    }

    public void InitiatePathfinding()
    {
        if (CanInitiatePathfinding())
        {
            rigidTrans.InitiatePathfinding(pathfindDestination, transform);
        }
    }

    public bool CanInitiatePathfinding()
    {
        return true;
    }

    public void CancelPathfinding()
    {
        rigidTrans.StopPathfinding();
    }
}
