using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class BallSimStateShot : BallSimState
{
    private static readonly FixedFloat SHOT_HEIGHT = (FixedFloat)0.25f;
    private const int DURATION_SHOT_FRAMES = 60;
    

    public override void OnEnter()
    {
        ballSim.Height = SHOT_HEIGHT;
        stateFrame = 0;
    }

    public override void _Update()
    {
        stateFrame++;

        if (stateFrame >= DURATION_SHOT_FRAMES)
        {
            ballSim.SwitchState(BallState.FREEFORM);
        }
        else
        {
            MoveHorizontal();
        }
    }
}
