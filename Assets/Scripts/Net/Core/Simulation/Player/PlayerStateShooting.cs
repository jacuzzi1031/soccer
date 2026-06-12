using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateShooting : PlayerSimState{
    private int _durationFrames;
    private int _elapsedFrames;

    public override void OnEnter() {
        _elapsedFrames = 0;

        _durationFrames = stateData.IsInstant ? 3 : 2;
    }

    public override void _Update() {
        _elapsedFrames++;

        if (_elapsedFrames >= _durationFrames) {
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