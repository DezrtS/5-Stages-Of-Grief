public interface IDodge
{
    Dodge Dodge { get; }
    bool IsDodging { get; }

    void InitiateDodge();
    bool CanInitiateDodge();
    void OnDodgeStart();
    void OnDodge();
    void OnDodgeEnd();
    void OnDodgeCancel(bool otherHasCancelled);
}