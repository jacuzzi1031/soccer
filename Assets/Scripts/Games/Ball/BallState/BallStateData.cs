


using Net.FixFloat;

public class BallStateData
{
    public FixedFloat LockDuration { get; set; }
    
    public static BallStateData Build()
    {
        return new BallStateData();
    }
    
    public BallStateData SetLockDuration(FixedFloat duration)
    {
        LockDuration = duration;
        return this;
    }
}

