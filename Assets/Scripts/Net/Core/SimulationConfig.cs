
using Net.FixFloat;

public class SimulationConfig
{
    public static readonly FixedFloat DeltaTime = (FixedFloat)(1f / 60f);
    
    public FixedFloat PlayerRadius = (FixedFloat)6.5f;

    public FixedFloat BallRadius = (FixedFloat)4.5f;

    public FixedFloat ballCaptureRadius = (FixedFloat)5.5f;

    public FixedFloat playerVerticalOffset = (FixedFloat)8f;

    public FixedFloat playervolleyRadius = (FixedFloat)8f;
}
