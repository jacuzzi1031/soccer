using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class PlayerStateTackling : PlayerSimState{
    private static readonly FixedFloat GROUND_FRICTION = (FixedFloat)120f;

    private const int PRIOR_RECOVERY_FRAMES = 30;

    private int _elapsedFrames;

    public override void OnEnter() {
        _elapsedFrames = 0;

        // playerSim.Velocity = _moveDirection * playerSim.Speed;
    }

    public override void _Update() {
        MoveHorizontal(GROUND_FRICTION);

        _elapsedFrames++;

        if (_elapsedFrames >= PRIOR_RECOVERY_FRAMES) {
            playerSim.SwitchState(PlayerState.RECOVERING);
        }
    }

    public override bool IsDamageEmitter() {
        return true;
    }
}