using UnityEngine;

public interface IPathfind
{
    bool IsPathfinding { get; }
    Vector3 PathfindDestination { get; set; }

    void TransferToPathfindingState(bool isPathfinding);
    void InitiatePathfinding();
    bool CanInitiatePathfinding();
    void OnPathfinding();
    void CancelPathfinding();
}