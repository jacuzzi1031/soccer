using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateTackling: PlayerSimState
{
    public const float GROUND_FRICTION = 120f;
    private float _elapsedTicks;
    private float DURATION_PRIOR_RECOVERY=0.5f;
    public override void OnEnter() {
        _elapsedTicks=0f;
        // playerSim.Velocity = _moveDirection * playerSim.Speed;
    }

    public override void _Update(float deltaTime) {
        MoveHorizontal(deltaTime,GROUND_FRICTION);
        _elapsedTicks+=deltaTime;
        if (_elapsedTicks >= DURATION_PRIOR_RECOVERY) {
            playerSim.SwitchState(PlayerState.RECOVERING);
        }
    }

    public override bool IsDamageEmitter() {
        return true;
    }
}
