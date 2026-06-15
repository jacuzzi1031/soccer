using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class PlayerStateBicycleKick : PlayerSimState
{
    private int _elapsedFrames;
    private int _durationFrames=12;

    public override void OnEnter() {
        _elapsedFrames = 0;
    }

    public override void _Update() {
        _elapsedFrames++;

        if (_elapsedFrames >= _durationFrames) {
            playerSim.SwitchState(PlayerState.RECOVERING);
        }
    }
    public override bool VolleyShot() {
        FixedVector2 destination = playerSim.GetFarTargetPosition();
        FixedVector2 direction = (destination - playerSim.Position).normalized;
        _ballSim.shoot( playerSim.Power * BONUS_POWER*direction);
        return true;
    }
    public override bool CanCarryBall() {
        return true;
    }
}
