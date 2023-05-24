using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Attack/Ranged Attack")]
public class RangedAttack : Attack
{
    [SerializeField] private GameObject projectilePrefab;

    public override void InitiateAttack()
    {

    }
}