using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateShooting: PlayerSimState
{
    private float _elapsedTicks;
    private float _durationTicks;
    public override void OnEnter() {
        _elapsedTicks = 0f;
        _durationTicks =stateData.IsInstant? 0.04f:0.03f; 
    }
    public override void _Update(float deltaTime) {
        _elapsedTicks+=deltaTime;

        if (_elapsedTicks >= _durationTicks)
        {
            OnAnimationComplete();
        }
    }
    public void OnAnimationComplete() {
        // if (playerSim.controlScheme == ControlScheme.CPU) {
        //     playerSim.SwitchState(PlayerState.RECOVERING);
        // }
        // else {
        //     playerSim.SwitchState(PlayerState.MOVING);
        // }
        playerSim.SwitchState(PlayerState.RECOVERING);
        _ballSim.shoot(stateData.ShotDirection * stateData.ShotPower);
    }
}


