using UnityEngine;

public interface IAim
{
    Vector3 AimDirection { get; }
    float RotationSpeed { get; }
    bool IsAiming { get; }

    bool Aim();

    bool CanAim();
}