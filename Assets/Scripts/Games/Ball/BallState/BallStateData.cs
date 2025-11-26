


public class BallStateData
{
    public int LockDuration { get; set; }
    
    public static BallStateData Build()
    {
        return new BallStateData();
    }
    
    public BallStateData SetLockDuration(int duration)
    {
        LockDuration = duration;
        return this;
    }
}

