using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Dodges/Basic Dodge")]
public class Dodge : ScriptableObject
{
    [Header("Dodge Variables")]
    [SerializeField] private string dodgeId;

    [Space(15)]
    [Header("Dodge Stages")]
    [SerializeField] private List<DodgeStateMovement> dodgeStateMovements = new List<DodgeStateMovement>();

    [Space(5)]
    [Header("Aiming")]
    [SerializeField] private bool hasAimingStage = true;
    [SerializeField] private float maxAimTime = 99;
    [SerializeField] private float dodgeRequiredChargeUpTime;
    [SerializeField] private float dodgeCancelCooldown;

    [Space(5)]
    [Header("Charging Up")]
    [SerializeField] private float chargingUpTime = 0;

    [Space(5)]
    [Header("Dodging")]
    [SerializeField] private bool hasDodgeStage = true;
    [SerializeField] private float dodgeTime = 0;

    [Space(5)]
    [Header("Cooling Down")]
    [SerializeField] private float coolingDownTime = 0;

    [Space(5)]
    [Header("Cooldown")]
    [SerializeField] private float dodgeCooldown;

    private float timeAimingStateStarted = int.MinValue;
    private float timeDodgingStateEnded = int.MinValue;

    private bool isClone;

    protected IDodge dodger;
    private Transform parentTransform;
    protected MovementController parentMovementController;

    private IEnumerator dodgeStateCoroutine;

    private DodgeState dodgeState = DodgeState.Idle;

    public string DodgeId { get { return dodgeId; } }

    public virtual Dodge Clone(Dodge clone, IDodge dodger, Transform parentTransform)
    {
        if (clone == null)
        {
            clone = CreateInstance(GetType()) as Dodge;
        }

        clone.dodgeId = dodgeId;
        clone.dodgeStateMovements = dodgeStateMovements;

        clone.hasAimingStage = hasAimingStage;
        clone.maxAimTime = maxAimTime;
        clone.dodgeRequiredChargeUpTime = dodgeRequiredChargeUpTime;
        clone.dodgeCancelCooldown = dodgeCancelCooldown;
        clone.chargingUpTime = chargingUpTime;
        clone.hasDodgeStage = hasDodgeStage;
        clone.dodgeTime = dodgeTime;
        clone.coolingDownTime = coolingDownTime;
        clone.dodgeCooldown = dodgeCooldown;

        clone.dodger = dodger;
        clone.parentTransform = parentTransform;
        clone.parentTransform.TryGetComponent(out clone.parentMovementController);

        clone.isClone = true;

        return clone;
    }

    public virtual void DestroyClone()
    {
        Destroy(this);
    }

    public virtual void TransferToDodgeState(DodgeState dodgeState)
    {
        if (CanInitiateDodgeState(dodgeState))
        {
            OnDodgeStateEnd(this.dodgeState);

            if (!hasAimingStage && dodgeState == DodgeState.Aiming)
            {
                dodger.InitiateDodgeState(DodgeState.ChargingUp);
                return;
            }
            else if (!hasDodgeStage && dodgeState == DodgeState.Dodging)
            {
                dodgeState = DodgeState.CoolingDown;
            }

            this.dodgeState = dodgeState;
            InitiateDodgeState(dodgeState);
        }
        else
        {
            if (this.dodgeState == DodgeState.ChargingUp && dodgeState == DodgeState.Dodging)
            {
                OnDodgeStateCancel(this.dodgeState, false);
            }
            else if (this.dodgeState == DodgeState.Aiming && dodgeState == DodgeState.ChargingUp)
            {
                OnDodgeStateCancel(this.dodgeState, false);
            }
        }
    }

    public virtual void InitiateDodgeState(DodgeState dodgeState)
    {
        dodger.TransferToDodgeState(dodgeState);

        OnDodgeStateStart(dodgeState);

        if (dodgeState == DodgeState.Idle)
        {
            return;
        }

        dodgeStateCoroutine = DodgeStateCoroutine(dodgeState);
        CoroutineRunner.Instance.StartCoroutine(dodgeStateCoroutine);
    }

    public virtual bool CanInitiateDodgeState(DodgeState dodgeState)
    {
        if (!isClone)
        {
            return false;
        }

        if (dodgeState == DodgeState.ChargingUp)
        {
            return (Time.timeSinceLevelLoad - timeAimingStateStarted >= dodgeRequiredChargeUpTime);
        }
        else if (dodgeState == DodgeState.Aiming)
        {
            return (Time.timeSinceLevelLoad - timeDodgingStateEnded >= dodgeCooldown);
        }

        return true;
    }

    public virtual void OnDodgeStateStart(DodgeState dodgeState)
    {
        dodger.OnDodgeStateStart(dodgeState);

        if (dodgeState == DodgeState.Aiming)
        {
            timeAimingStateStarted = Time.timeSinceLevelLoad;
        }  
    }

    public virtual void OnDodgeState(DodgeState dodgeState, float timeSinceStateStarted)
    {
        dodger.OnDodgeState(dodgeState);

        switch (dodgeState)
        {
            case DodgeState.Idle:
                Debug.LogWarning("Idle On Attack State");
                break;
            case DodgeState.Aiming:
                if (maxAimTime < timeSinceStateStarted)
                {
                    TransferToDodgeState(DodgeState.ChargingUp);
                }
                break;
            case DodgeState.ChargingUp:
                if (chargingUpTime < timeSinceStateStarted)
                {
                    TransferToDodgeState(DodgeState.Dodging);
                }
                break;
            case DodgeState.Dodging:
                if (dodgeTime < timeSinceStateStarted)
                {
                    TransferToDodgeState(DodgeState.CoolingDown);
                }
                break;
            case DodgeState.CoolingDown:
                if (coolingDownTime < timeSinceStateStarted)
                {
                    TransferToDodgeState(DodgeState.Idle);
                }
                break;
        }
    }

    public virtual void OnDodgeStateEnd(DodgeState dodgeState)
    {
        dodger.OnDodgeStateEnd(dodgeState);

        CoroutineRunner.Instance.StopCoroutine(dodgeStateCoroutine);

        if (dodgeState == DodgeState.Dodging)
        {
            timeDodgingStateEnded = Time.timeSinceLevelLoad;
        }
    }

    public virtual void OnDodgeStateCancel(DodgeState dodgeState, bool otherHasCancelled)
    {
        if (!otherHasCancelled)
        {
            dodger.OnDodgeStateCancel(dodgeState, true);
        }

        this.dodgeState = DodgeState.Idle;
        CoroutineRunner.Instance.StopCoroutine(dodgeStateCoroutine);

        if (dodgeState == DodgeState.Dodging)
        {
            timeDodgingStateEnded = Time.timeSinceLevelLoad - dodgeCooldown + dodgeCancelCooldown;
        }
    }

    public IEnumerator DodgeStateCoroutine(DodgeState dodgeState)
    {
        float timeStateStarted = Time.timeSinceLevelLoad;

        bool useMovementState = false;
        StateMovement stateMovement = null;
        float stateTimeLength = 0;

        if (parentMovementController != null)
        {
            foreach (DodgeStateMovement dodgeStateMovement in dodgeStateMovements)
            {
                if (dodgeStateMovement.State == dodgeState)
                {
                    useMovementState = true;
                    stateMovement = dodgeStateMovement.Movement;

                    switch (dodgeState)
                    {
                        case DodgeState.Aiming:
                            stateTimeLength = maxAimTime;
                            break;
                        case DodgeState.ChargingUp:
                            stateTimeLength = chargingUpTime;
                            break;
                        case DodgeState.Dodging:
                            stateTimeLength = dodgeTime;
                            break;
                        case DodgeState.CoolingDown:
                            stateTimeLength = coolingDownTime;
                            break;
                        default:
                            break;
                    }

                    break;
                }
            }
        }

        while (true)
        {
            yield return new WaitForFixedUpdate();

            if (useMovementState)
            {
                parentMovementController.SetVelocity(stateMovement.GetStateCurrentVelocity(Time.timeSinceLevelLoad - timeStateStarted, stateTimeLength, parentTransform, parentMovementController.GetVelocity()));
            }

            OnDodgeState(dodgeState, Time.timeSinceLevelLoad - timeStateStarted);
        }
    }
}

[Serializable]
public class DodgeStateMovement
{
    [SerializeField] private DodgeState state;
    [SerializeField] private StateMovement movement;

    public DodgeState State { get { return state; } }
    public StateMovement Movement { get { return movement; } }
}