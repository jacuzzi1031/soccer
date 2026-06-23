using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class PlayerStateBicycleKick : PlayerSimState
{
    private int _durationFrames=16;

    public override void OnEnter() {
        stateFrame = 0;
    }

    public override void _Update() {
        stateFrame++;

        if (stateFrame >= _durationFrames) {
            playerSim.SwitchState(PlayerState.RECOVERING);
        }
    }
    public override bool VolleyShot() {
        FixedVector2 destination = playerSim.GetFarTargetPosition();
        FixedVector2 direction = (destination - playerSim.Position).normalized;
        _eventBus.Publish( new PlayStyleShowSignal( playerSim.playerId, PlayerState.BICYCLE_KICK) );
        _ballSim.shoot( playerSim.Power * BONUS_POWER*direction);
        return true;
    }
    public override bool CanCarryBall() {
        return true;
    }
}
