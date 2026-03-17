using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateVolleyKick : PlayerSimState
{
    private float _elapsedTicks;
    // private float _durationTicks=0.2f;
    private float _durationTicks=0.4f;

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
    public override bool VolleyShot() {
        Vector2 destination = playerSim.GetFarTargetPosition();
        Vector2 direction = (destination - playerSim.Position).normalized;
        _ballSim.shoot( playerSim.Power * BONUS_POWER*direction);
        return true;
    }
}
