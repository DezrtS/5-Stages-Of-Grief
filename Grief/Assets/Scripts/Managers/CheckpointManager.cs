public class CheckpointManager : Singleton<CheckpointManager>
{
    private Checkpoint activeCheckpoint;

    public Checkpoint GetActiveCheckpoint()
    {
        return activeCheckpoint;
    }

    public void SetActiveCheckpoint(Checkpoint checkpoint)
    {
        if (activeCheckpoint == null)
        {
            activeCheckpoint = checkpoint;
            return;
        }
        
        if (checkpoint.CheckpointPriority >= activeCheckpoint.CheckpointPriority)
        {
            // Send Event for new Checkpoint set
            activeCheckpoint = checkpoint;
        }
    }
}