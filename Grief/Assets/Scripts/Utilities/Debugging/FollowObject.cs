using UnityEngine;

public class FollowObject : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private bool lockY;
    private Vector3 offset;
    private Transform trans;

    private void Awake()
    {
        trans = transform;
        offset = trans.position - followTarget.position;
    }

    private void LateUpdate()
    {
        Vector3 position = followTarget.position + offset;
        if (lockY)
        {
            position.y = trans.position.y;
        }
        trans.position = position;
    }
}