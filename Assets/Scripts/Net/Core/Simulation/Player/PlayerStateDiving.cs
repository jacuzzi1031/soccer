using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using Unity.Mathematics;
using UnityEngine;

public class PlayerStateDiving: PlayerSimState
{
    private const int DURATION_FRAMES = 30;

    private int _elapsedFrames;
    public FixedFloat timeStartDive;

    public override void OnEnter()
    {
        FixedFloat goalLineX = playerSim.spawnPosition.x;

        FixedVector2 ballPos = _ballSim.Position;
        FixedVector2 ballVel = _ballSim.Velocity;

        FixedVector2 targetDive;

        if (FixedFloat.Abs(ballVel.x) > (FixedFloat)0.01f)
        {
            FixedFloat t = (goalLineX - ballPos.x) / ballVel.x;

            if (t > FixedFloat.Zero)
            {
                FixedFloat impactY = ballPos.y + ballVel.y * t;
                targetDive = new FixedVector2(goalLineX, impactY);
            }
            else
            {
                targetDive = ballPos;
            }
        }
        else
        {
            targetDive = ballPos;
        }

        FixedVector2 moveDir =
            (targetDive - playerSim.Position).normalized;

        playerSim.Velocity =
            moveDir * playerSim.Speed;

        diveDirY = playerSim.Velocity.y;

        _elapsedFrames = 0;
    }

    public override void _Update()
    {
        _elapsedFrames++;

        if (FixedFloat.Abs(playerSim.Velocity.y) < (FixedFloat)0.1f)
        {
            playerSim.Velocity =
                new FixedVector2(
                    FixedFloat.Zero,
                    diveDirY * (FixedFloat)1.1f);
        }

        playerSim.Position +=
            playerSim.Velocity * SimulationConfig.DeltaTime;

        if (_elapsedFrames >= DURATION_FRAMES)
        {
            playerSim.SwitchState(PlayerState.RECOVERING);
        }
    }
}
