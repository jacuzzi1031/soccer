using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateChestControl: PlayerSimState
{
    private const float DURATION_CONTROL = 0.5f;
    private float _elapsedTicks;
    public override void OnEnter() {

        playerSim.Velocity=Vector2.zero;
        _elapsedTicks = 0f;
    }

    public override void _Update(float deltaTime) {
        _elapsedTicks += deltaTime;
        if (_elapsedTicks > DURATION_CONTROL)
        {
            playerSim.SwitchState(PlayerState.MOVING);
        }
    }
}
