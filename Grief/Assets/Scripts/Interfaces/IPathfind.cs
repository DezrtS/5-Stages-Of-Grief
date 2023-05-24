using UnityEngine;

public interface IPathfind
{
    Vector3 PathfindPosition { get; set; }

    bool Pathfind();
}
