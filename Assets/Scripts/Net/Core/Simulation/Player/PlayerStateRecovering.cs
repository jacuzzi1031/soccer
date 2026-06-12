using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class PlayerStateRecovering: PlayerSimState
{
    private const int DURATION_FRAMES = 24;

    private int _elapsedFrames;

    public override void OnEnter()
    {
        playerSim.Velocity = FixedVector2.Zero;
        _elapsedFrames = 0;
    }

    public override void _Update()
    {
        _elapsedFrames++;

        if (_elapsedFrames >= DURATION_FRAMES)
        {
            playerSim.SwitchState(PlayerState.MOVING);
        }
    }
}
