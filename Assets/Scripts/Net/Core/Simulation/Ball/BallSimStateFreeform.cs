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

    

    // public void MoveVertical(float deltaTime,float bounciness = 0.0f)
    // {   
    //     //默认height不会是0，而是-0.888，导致一直削弱速度
    //     if (ballSim.height > 0 || ballSim.heightVelocity > 0) {
    //         ballSim.heightVelocity -= GRAVITY * deltaTime;//heightVelocity 每帧位移
    //         ballSim.height += ballSim.heightVelocity;
    //         if (ballSim.height < 0)
    //         {
    //             ballSim.height = 0;
    //             if (ballSim.heightVelocity < 0)
    //             {
    //                 ballSim.heightVelocity = -ballSim.heightVelocity * bounciness;
    //                 ballSim.Velocity *= bounciness;
    //             }
    //         }
    //     }
    // }
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
