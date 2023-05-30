using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Attacks/Generic Attack")]
public class Attack : ScriptableObject
{
    [Header("Attack Stats")]
    [SerializeField] private float attackRange;
    [SerializeField] private float attackDamage;
    [SerializeField] private float attackCooldown;

    [Space(10)]
    [Header("Projectile Details")]
    [SerializeField] private bool spawnsProjectile;
    [SerializeField] private GameObject projectilePrefab;

    public float AttackRange { get { return attackRange; } }
    public float AttackDamage { get { return attackDamage; } }
    public float AttackCooldown { get { return attackCooldown; } }

    public virtual void InitiateAttack<T>(T entity, Transform transform) where T : IAttack
    {

    }

    public virtual void CanAttack()
    {

    }

    public virtual IEnumerator HandleAttack()
    {
        yield return new WaitForSeconds(1);
    }
}