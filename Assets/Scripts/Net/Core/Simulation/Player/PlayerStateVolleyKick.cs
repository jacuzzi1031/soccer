using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class PlayerStateVolleyKick : PlayerSimState{
    private const int DURATION_FRAMES = 16;

    private int _elapsedFrames;

    public override void OnEnter() {
        _elapsedFrames = 0;
    }

    public override void _Update() {
        _elapsedFrames++;

        if (_elapsedFrames >= DURATION_FRAMES) {
            playerSim.SwitchState(PlayerState.MOVING);
        }
    }

    public override bool VolleyShot() {
        FixedVector2 destination =
            playerSim.GetFarTargetPosition();

        FixedVector2 direction =
            (destination - playerSim.Position).normalized;

        _ballSim.shoot(
            direction * playerSim.Power * BONUS_POWER);

        return true;
    }

    public override bool CanCarryBall() {
        return true;
    }
}