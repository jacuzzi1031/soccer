using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class PlayerStateChestControl: PlayerSimState
{
    private const int DURATION_CONTROL_FRAMES = 30;



    public override void OnEnter()
    {
        playerSim.Velocity = FixedVector2.Zero;
        stateFrame = 0;
    }

    public override void _Update()
    {
        stateFrame++;

        if (stateFrame >= DURATION_CONTROL_FRAMES)
        {
            playerSim.SwitchState(PlayerState.MOVING);
        }
    }
}
