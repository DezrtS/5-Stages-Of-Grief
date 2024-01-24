using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Projectile Data")]
public class ProjectileData : ScriptableObject
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private string projectileId;
    [SerializeField] private float damage;
    [SerializeField] private float knockbackPower;
    [SerializeField] private float fireSpeed;
    [SerializeField] private float projectileLifespan;

    public GameObject ProjectilePrefab { get { return projectilePrefab; } }
    public string ProjectileId { get { return projectileId; } }
    public float Damage { get { return damage; } }
    public float KnockbackPower { get { return knockbackPower; } }
    public float FireSpeed { get {  return fireSpeed; } }
    public float ProjectileLifespan { get {  return projectileLifespan; } }
}