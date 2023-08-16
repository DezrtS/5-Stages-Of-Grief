using System.Collections;
using UnityEngine;

public abstract class Dodge : ScriptableObject
{
    // dodgeSpeed will probably be changed to dodgeDistance
    [Header("Dodge Variables")]
    [SerializeField] protected float dodgeSpeed;
    [SerializeField] private float dodgeTime;
    [SerializeField] private float dodgeCooldown;

    private IEnumerator dodgeCoroutine;
    private bool isClone;
    private float timeDodgeStarted = 0;
    private float timeLastDodgeEnded = 0;

    private bool isDodging;

    protected RigidTransform parentRigidTransform;

    public float DodgeSpeed { get { return dodgeSpeed; } }
    public float DodgeTime { get { return dodgeTime; } }
    public float DodgeCooldown { get { return dodgeCooldown; } }

    public virtual Dodge Clone(Dodge clone)
    {
        if (clone == null)
        {
            clone = CreateInstance(GetType()) as Dodge;
        }

        clone.dodgeSpeed = dodgeSpeed;
        clone.dodgeTime = dodgeTime;
        clone.dodgeCooldown = dodgeCooldown;

        clone.isClone = true;

        return clone;
    }

    public virtual void InitiateDodge<T>(Vector3 dodgeDirection, T dodger, RigidTransform rigidTransform) where T : IDodge
    {
        if (CanInitiateDodge())
        {
            Debug.Log("Dodge Triggered");

            if (isDodging)
            {
                // May Alter This Later
                OnDodgeCancel(dodger, false);
            }

            parentRigidTransform = rigidTransform;
            isDodging = true;

            OnDodgeStart(dodgeDirection, dodger);

            dodgeCoroutine = DodgeCoroutine(dodgeDirection, dodger);
            CoroutineRunner.Instance.StartCoroutine(dodgeCoroutine);
        }
    }

    public virtual bool CanInitiateDodge()
    {
        if (!isClone)
        {
            return false;
        }

        return (Time.timeSinceLevelLoad - timeLastDodgeEnded >= dodgeCooldown || timeLastDodgeEnded == 0);
    }

    public virtual void OnDodgeStart<T>(Vector3 dodgeDirection, T dodger) where T : IDodge
    {
        dodger.OnDodgeStart();
    }

    public virtual void OnDodge<T>(Vector3 dodgeDirection, T dodger, float timeSinceDodgeStarted) where T : IDodge
    {
        dodger.OnDodge();

        if (timeSinceDodgeStarted > dodgeTime)
        {
            OnDodgeEnd(dodgeDirection, dodger);
        }
    }

    public virtual void OnDodgeEnd<T>(Vector3 dodgeDirection, T dodger) where T : IDodge
    {
        dodger.OnDodgeEnd();
        isDodging = false;
        CoroutineRunner.Instance.StopCoroutine(dodgeCoroutine);
        timeLastDodgeEnded = Time.timeSinceLevelLoad;
    }
 
    public virtual void OnDodgeCancel<T>(T dodger, bool otherHasCancelled) where T : IDodge
    {
        if (!otherHasCancelled)
        {
            dodger.OnDodgeCancel(true);
        }

        isDodging = false;
        CoroutineRunner.Instance.StopCoroutine(dodgeCoroutine);
        timeLastDodgeEnded = Time.timeSinceLevelLoad;
    }

    public IEnumerator DodgeCoroutine<T>(Vector3 dodgeDirection, T dodger) where T : IDodge
    {
        timeDodgeStarted = Time.timeSinceLevelLoad;

        while (true)
        {
            yield return new WaitForFixedUpdate();
            OnDodge(dodgeDirection, dodger, Time.timeSinceLevelLoad - timeDodgeStarted);
        }
    }
}