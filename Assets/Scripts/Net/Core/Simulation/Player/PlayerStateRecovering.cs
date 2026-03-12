using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateRecovering: PlayerSimState
{
    private float _elapsedTicks;
    private float _durationTicks=0.1f;

    public override void OnEnter() {
        _elapsedTicks = 0f;
    }

    public override void _Update(float deltaTime) {
        _elapsedTicks+=deltaTime;

        if (_elapsedTicks >= _durationTicks)
        {
            playerSim.SwitchState(PlayerState.MOVING);
        }
    }
}
