using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class BallSimStateFreeform : BallSimState
{
    private bool canCarried = false;
    private int lockDurationFrames;
    public override void OnEnter()
    {
        canCarried = false;
        stateFrame = 0;
        lockDurationFrames =
            (int)(stateData.LockDuration / SimulationConfig.DeltaTime);
    }

    public override void _Update()
    {
        stateFrame++;
        SetLockDuration();
        MoveVertical(BOUNCINESS);
        MoveHorizontal();
    }

    private void SetLockDuration()
    {
        if (!canCarried)
        {

            if (stateFrame >= lockDurationFrames)
            {
                canCarried = true;
            }
        }
    }
    public override bool CanCarriedBall() {
        return canCarried;
    }
    public override bool CanAirInteract() {
        return true; 
    }
}
