using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateShooting: PlayerSimState
{
    private int _elapsedTicks;
    private int _durationTicks;
    public override void OnEnter() {
        _elapsedTicks = 0;
        _durationTicks =stateData.IsInstant? 3:2; 
    }

    public override void _Update(float deltaTime) {
        _elapsedTicks++;

        if (_elapsedTicks >= _durationTicks)
        {
            OnAnimationComplete();
        }
    }
    public void OnAnimationComplete() {
        if (playerSim.controlScheme == ControlScheme.CPU) {
            playerSim.SwitchState(PlayerState.RECOVERING);
        }
        else {
            playerSim.SwitchState(PlayerState.MOVING);
        }
        // ShootBall
        _commandBuffer.Enqueue(new SimulationCommand
        {
            Type = SimulationCommandType.BallShoot,
            ShotVelocity=stateData.ShotDirection * stateData.ShotPower
        });
    }
}


