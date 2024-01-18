using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private int checkpointPriority = 0;
    public int CheckpointPriority { get { return checkpointPriority; } }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CheckpointManager.Instance.SetActiveCheckpoint(this);
        }
    }
}