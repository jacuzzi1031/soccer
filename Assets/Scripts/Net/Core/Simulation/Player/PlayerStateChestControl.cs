using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class PlayerStateChestControl: PlayerSimState
{
    private const int DURATION_CONTROL_FRAMES = 30;

    private int _controlFrames;

    public override void OnEnter()
    {
        playerSim.Velocity = FixedVector2.Zero;
        _controlFrames = 0;
    }

    public override void _Update()
    {
        _controlFrames++;

        if (_controlFrames >= DURATION_CONTROL_FRAMES)
        {
            playerSim.SwitchState(PlayerState.MOVING);
        }
    }
}
