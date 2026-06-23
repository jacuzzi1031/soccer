using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class PlayerStateRecovering: PlayerSimState
{
    private const int DURATION_FRAMES = 24;
    

    public override void OnEnter()
    {
        playerSim.Velocity = FixedVector2.Zero;
        stateFrame = 0;
    }

    public override void _Update()
    {
        stateFrame++;

        if (stateFrame >= DURATION_FRAMES)
        {
            playerSim.SwitchState(PlayerState.MOVING);
        }
    }
}
