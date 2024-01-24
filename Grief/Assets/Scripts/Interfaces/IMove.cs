using UnityEngine;

public interface IMove
{
    public Vector3 GetMovementInput();
    public Vector3 GetRotationInput();
}