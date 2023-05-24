using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Dodge")]
public class Dodge : ScriptableObject
{
    [SerializeField] private float dodgeSpeed;
    [SerializeField] private float dodgeTime;
    [SerializeField] private float dodgeCooldown;

    public float DodgeSpeed { get { return dodgeSpeed; } }
    public float DodgeTime { get { return dodgeTime; } }
    public float DodgeCooldown { get { return dodgeCooldown; } }
}
