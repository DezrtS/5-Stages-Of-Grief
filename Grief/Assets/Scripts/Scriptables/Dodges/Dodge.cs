using System.Collections;
using UnityEngine;

// Add different dodge stages like in attacks
public abstract class Dodge : ScriptableObject
{
    [Header("Dodge Variables")]
    [SerializeField] private string dodgeId;

    [SerializeField] private float dodgeDistance;
    [SerializeField] private float dodgeTime;
    [SerializeField] private float dodgeCooldown;

    private IEnumerator dodgeCoroutine;
    private bool isClone;
    private float timeDodgeStarted = 0;
    private float timeLastDodgeEnded = 0;

    private bool isDodging;

    protected IDodge dodger;
    protected MovementController parentMovementController;

    public string DodgeId { get { return dodgeId; } }

    public virtual Dodge Clone(Dodge clone, IDodge dodger, MovementController parentMovementController)
    {
        if (clone == null)
        {
            clone = CreateInstance(GetType()) as Dodge;
        }

        clone.dodgeDistance = dodgeDistance;
        clone.dodgeTime = dodgeTime;
        clone.dodgeCooldown = dodgeCooldown;

        clone.dodger = dodger;
        clone.parentMovementController = parentMovementController;

        clone.isClone = true;

        return clone;
    }

    public virtual void DestroyClone()
    {
        Destroy(this);
    }

    public virtual void InitiateDodge(Vector3 dodgeDirection, Vector3 directionInput)
    {
        if (CanInitiateDodge())
        {

            if (isDodging)
            {
                // May Alter This Later
                OnDodgeCancel(false);
            }

            isDodging = true;

            OnDodgeStart(dodgeDirection, directionInput);

            dodgeCoroutine = DodgeCoroutine(dodgeDirection);
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

    public virtual void OnDodgeStart(Vector3 dodgeDirection, Vector3 directionInput)
    {
        dodger.OnDodgeStart();
    }

    public virtual void OnDodge(Vector3 dodgeDirection, float timeSinceDodgeStarted)
    {
        dodger.OnDodge();

        if (timeSinceDodgeStarted > dodgeTime)
        {
            OnDodgeEnd(dodgeDirection);
        }
    }

    public virtual void OnDodgeEnd(Vector3 dodgeDirection)
    {
        dodger.OnDodgeEnd();
        isDodging = false;
        CoroutineRunner.Instance.StopCoroutine(dodgeCoroutine);
        timeLastDodgeEnded = Time.timeSinceLevelLoad;
    }
 
    public virtual void OnDodgeCancel(bool otherHasCancelled)
    {
        if (!otherHasCancelled)
        {
            dodger.OnDodgeCancel(true);
        }

        isDodging = false;
        CoroutineRunner.Instance.StopCoroutine(dodgeCoroutine);
        timeLastDodgeEnded = Time.timeSinceLevelLoad;
    }

    public IEnumerator DodgeCoroutine(Vector3 dodgeDirection)
    {
        timeDodgeStarted = Time.timeSinceLevelLoad;

        while (true)
        {
            yield return new WaitForFixedUpdate();
            OnDodge(dodgeDirection, Time.timeSinceLevelLoad - timeDodgeStarted);
        }
    }

    public float GetDodgeSpeed()
    {
        return dodgeDistance / dodgeTime;
    }
}