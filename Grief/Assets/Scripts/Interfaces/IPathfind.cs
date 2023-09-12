using UnityEngine;

public enum PathingState
{
    Idle,
    Pathfinding,
    Stopping
}

public interface IPathfind
{
    bool IsPathfinding { get; }
    Vector3 PathfindDestination { get; set; }

    void TransferToPathfindingState(PathingState pathingState);
    void InitiatePathfinding();
    bool CanInitiatePathfinding();
    void OnPathfinding();
    void CancelPathfinding();
}