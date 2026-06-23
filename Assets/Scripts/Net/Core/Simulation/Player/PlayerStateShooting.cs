using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateShooting : PlayerSimState{
    private int _durationFrames;

    public override void OnEnter() {
        stateFrame = 0;

        _durationFrames = stateData.IsInstant ? 3 : 2;
    }

    public override void _Update() {
        stateFrame++;

        if (stateFrame >= _durationFrames) {
            OnAnimationComplete();
        }
    }

    public void OnAnimationComplete() {
        playerSim.SwitchState(PlayerState.RECOVERING);

        _ballSim.shoot(
            stateData.ShotDirection *
            stateData.ShotPower);
    }
}