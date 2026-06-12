using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class BallSimStateShot : BallSimState
{
    private static readonly FixedFloat SHOT_HEIGHT = (FixedFloat)0.25f;
    private const int DURATION_SHOT_FRAMES = 60;
    
    private int shotFrameCount;
    

    public override void OnEnter()
    {
        ballSim.height = SHOT_HEIGHT;
        shotFrameCount = 0;
    }

    public override void _Update()
    {
        shotFrameCount++;

        if (shotFrameCount >= DURATION_SHOT_FRAMES)
        {
            ballSim.SwitchState(BallState.FREEFORM);
        }
        else
        {
            MoveHorizontal();
        }
    }
}
