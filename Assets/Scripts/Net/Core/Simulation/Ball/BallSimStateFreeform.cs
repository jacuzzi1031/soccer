using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class BallSimStateFreeform : BallSimState
{
    private FixedFloat lockDurationCheckTimer = FixedFloat.Zero;
    private bool canCarried = false;

    public override void OnEnter()
    {
        canCarried = false;
        lockDurationCheckTimer = FixedFloat.Zero;
    }

    public override void _Update()
    {
        SetLockDuration();
        MoveVertical(BOUNCINESS);
        MoveHorizontal();
    }

    private void SetLockDuration()
    {
        if (!canCarried)
        {
            lockDurationCheckTimer += SimulationConfig.DeltaTime;

            if (lockDurationCheckTimer >= stateData.LockDuration)
            {
                canCarried = true;
                lockDurationCheckTimer = FixedFloat.Zero;
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
