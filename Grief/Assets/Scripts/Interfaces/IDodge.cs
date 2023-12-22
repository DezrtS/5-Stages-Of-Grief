public enum DodgeState
{
    Idle,
    Aiming,
    ChargingUp,
    Dodging,
    CoolingDown
}

public interface IDodge
{
    DodgeHolder DodgeHolder { get; }
    bool IsDodging { get; }
    DodgeState DodgeState { get; }

    void TransferToDodgeState(DodgeState dodgeState);
    void InitiateDodgeState(DodgeState dodgeState);
    bool CanInitiateDodgeState(DodgeState dodgeState, string dodgeId);
    void OnDodgeStateStart(DodgeState dodgeState);
    void OnDodgeState(DodgeState dodgeState);
    void OnDodgeStateEnd(DodgeState dodgeState);
    void OnDodgeStateCancel(DodgeState dodgeState, bool otherHasCancelled);
}