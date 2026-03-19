using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSimStateFreeform : BallSimState
{
    private float LockDurationCheckTimer = 0f;
    private bool CanCarried=false;
    public override  void OnEnter() {
        CanCarried=false;
        LockDurationCheckTimer = 0f;
    }
    public override void _Update(float deltaTime) {
        SetLockDuration(deltaTime);
        MoveVertical(deltaTime,BOUNCINESS);
        MoveHorizontal(deltaTime);
    }
    private void SetLockDuration(float deltaTime) {
        if (!CanCarried&&LockDurationCheckTimer < stateData.LockDuration) {
            LockDurationCheckTimer+= deltaTime;
        }
        else {
            LockDurationCheckTimer = 0f;
            CanCarried = true;
        }
    }
    
    public override bool CanCarriedBall() {
        return CanCarried;
    }
    public override bool CanAirInteract() {
        return true; 
    }
}
