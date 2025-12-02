


public class BallStateData
{
    public float LockDuration { get; set; }
    
    public static BallStateData Build()
    {
        return new BallStateData();
    }
    
    public BallStateData SetLockDuration(float duration)
    {
        LockDuration = duration;
        return this;
    }
}

