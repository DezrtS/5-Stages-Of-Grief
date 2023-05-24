public interface IDodge
{
    Dodge Dodge { get; }
    bool IsDodging { get; }

    void InitiateDodge();

    bool CanDodge();

    void OnDodgeStart();

    void OnDodgeEnd();
}