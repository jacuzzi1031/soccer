using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSimStateShot : BallSimState
{
    private const float SHOT_SPRITE_SCALE = 0.8f;
    private const float SHOT_HEIGHT = 2.5f;
    private const float DURATION_SHOT = 1.0f;
    private float timeSinceShot;
    public override void OnEnter() { 
        ballSim.height = SHOT_HEIGHT;
        timeSinceShot = 0f;
    }

    public override void _Update(float deltaTime) {

        timeSinceShot += deltaTime;

        if (timeSinceShot >= DURATION_SHOT) {
            ballSim.SwitchState(BallState.FREEFORM);
        }
        else {
            MoveHorizontal(deltaTime);
        }
    }
}
