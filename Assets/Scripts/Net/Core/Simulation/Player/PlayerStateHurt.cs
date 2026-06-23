using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class PlayerStateHurt: PlayerSimState
{
    private static readonly FixedFloat HURT_HEIGHT_VELOCITY = (FixedFloat)50f;
    private static readonly FixedFloat BALL_TUMBLE_SPEED = (FixedFloat)20f;

    private const int DURATION_HURT_FRAMES = 60;
    

    public override void OnEnter()
    {
        playerSim.HeightVelocity = HURT_HEIGHT_VELOCITY;
        playerSim.Height = (FixedFloat)0.1f;

        FixedVector2 tumbleDir = stateData.MoveDir;

        _ballSim.Tumble(
            tumbleDir * BALL_TUMBLE_SPEED);

        stateFrame = 0;
    }

    public override void _Update()
    {
        stateFrame++;

        MoveHorizontal( AIR_FRICTION);

        if (stateFrame >= DURATION_HURT_FRAMES)
        {
            playerSim.SwitchState(PlayerState.RECOVERING);
        }
    }
}
