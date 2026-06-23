using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class PlayerStateTackling : PlayerSimState{
    private static readonly FixedFloat tackle_FRICTION = (FixedFloat)140f;

    private const int PRIOR_RECOVERY_FRAMES = 30;
    

    public override void OnEnter() {
        stateFrame = 0;

        // playerSim.Velocity = _moveDirection * playerSim.Speed;
    }

    public override void _Update() {
        MoveHorizontal(tackle_FRICTION);

        stateFrame++;

        if (stateFrame >= PRIOR_RECOVERY_FRAMES) {
            playerSim.SwitchState(PlayerState.RECOVERING);
        }
    }

    public override bool IsDamageEmitter() {
        return true;
    }
}