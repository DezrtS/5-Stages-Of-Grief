using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Dodges/Generic Dodge")]
public class Dodge : ScriptableObject
{
    [Header("Dodge Stats")]
    [SerializeField] private float dodgeSpeed;
    [SerializeField] private float dodgeTime;
    [SerializeField] private float dodgeCooldown;

    private Coroutine dodgeCoroutine;
    private bool isClone;
    private float timeSinceLastDodge = 0;

    public float DodgeSpeed { get { return dodgeSpeed; } }
    public float DodgeTime { get { return dodgeTime; } }
    public float DodgeCooldown { get { return dodgeCooldown; } }

    public Dodge Clone()
    {
        Dodge clone = ScriptableObject.CreateInstance<Dodge>();

        clone.dodgeSpeed = dodgeSpeed;
        clone.dodgeTime = dodgeTime;
        clone.dodgeCooldown = dodgeCooldown;

        clone.isClone = true;

        return clone;
    }

    public virtual void InitiateDodge<T>(T entity, CharacterController characterController, Vector3 dodgeDirection) where T : IDodge
    {
        entity.OnDodgeStart();
        dodgeCoroutine = CoroutineRunner.Instance.StartCoroutine(HandleDodge(entity, characterController, dodgeDirection));
    }

    public virtual bool CanDodge()
    {
        return (Time.timeSinceLevelLoad - timeSinceLastDodge >= dodgeCooldown || timeSinceLastDodge == 0) && isClone;
    }

    public virtual IEnumerator HandleDodge<T>(T entity, CharacterController characterController, Vector3 dodgeDirection) where T : IDodge
    {
        float elapsedTime = 0;

        while (elapsedTime < dodgeTime)
        {
            if (Time.timeScale > 0)
            {
                characterController.Move(dodgeSpeed * dodgeDirection * Time.deltaTime);

                elapsedTime += Time.deltaTime;
            }

            yield return null;
        }

        timeSinceLastDodge = Time.timeSinceLevelLoad;
        entity.OnDodgeEnd();
    }
}
