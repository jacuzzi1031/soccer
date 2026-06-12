using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class PlayerStateHurt: PlayerSimState
{
    private static readonly FixedFloat HURT_HEIGHT_VELOCITY = (FixedFloat)50f;
    private static readonly FixedFloat BALL_TUMBLE_SPEED = (FixedFloat)20f;

    private const int DURATION_HURT_FRAMES = 60;

    private int _hurtFrames;

    public override void OnEnter()
    {
        playerSim.HeightVelocity = HURT_HEIGHT_VELOCITY;
        playerSim.Height = (FixedFloat)0.1f;

        FixedVector2 tumbleDir = stateData.MoveDir;

        _ballSim.Tumble(
            tumbleDir * BALL_TUMBLE_SPEED);

        _hurtFrames = 0;
    }

    public override void _Update()
    {
        _hurtFrames++;

        MoveHorizontal( AIR_FRICTION);

        if (_hurtFrames >= DURATION_HURT_FRAMES)
        {
            playerSim.SwitchState(PlayerState.RECOVERING);
        }
    }
}
