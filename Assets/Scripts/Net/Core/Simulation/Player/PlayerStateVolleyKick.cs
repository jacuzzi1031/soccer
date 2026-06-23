using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class PlayerStateVolleyKick : PlayerSimState{
    private const int DURATION_FRAMES = 16;



    public override void OnEnter() {
        stateFrame = 0;
    }

    public override void _Update() {
        stateFrame++;

        if (stateFrame >= DURATION_FRAMES) {
            playerSim.SwitchState(PlayerState.MOVING);
        }
    }

    public override bool VolleyShot() {
        FixedVector2 destination =
            playerSim.GetFarTargetPosition();

        FixedVector2 direction =
            (destination - playerSim.Position).normalized;
        _eventBus.Publish(
            new PlayStyleShowSignal(
                playerSim.playerId,
                PlayerState.VOLLEY_KICK)
        );
        _ballSim.shoot(
            direction * playerSim.Power * BONUS_POWER);

        return true;
    }

    public override bool CanCarryBall() {
        return true;
    }
}