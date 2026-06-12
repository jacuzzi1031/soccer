using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class BallSimStateCarried : BallSimState
{
    public override void OnEnter() {
        ballSim.Velocity=FixedVector2.Zero;
        ballSim.heightVelocity=FixedFloat.Zero;
        if (ballSim.height > (FixedFloat)0.6f) {
            ballSim.height -= (FixedFloat)0.2f;
        }
    }
    private FixedFloat dribbleTime = FixedFloat.Zero;

    private static readonly FixedFloat DRIBBLE_FREQUENCY = (FixedFloat)10;
    private static readonly FixedFloat DRIBBLE_INTENSITY = (FixedFloat)3;

    private static readonly FixedVector2 OFFSET_FROM_PLAYER =
        new FixedVector2(
            (FixedFloat)10.8f,
            (FixedFloat)4f
        );

    public override void _Update()
    {
        dribbleTime += SimulationConfig.DeltaTime;

        FixedFloat vx = FixedFloat.Zero;

        if (ballSim.carrier.Velocity.x != FixedFloat.Zero)
        {
            vx =
                CosTable.Cos(
                    dribbleTime * DRIBBLE_FREQUENCY
                ) * DRIBBLE_INTENSITY;
        }

        bool facingRight = ballSim.carrier.HeadingRight;

        ballSim.Position =
            ballSim.carrier.Position +
            new FixedVector2(
                facingRight
                    ? OFFSET_FROM_PLAYER.x + vx
                    : -OFFSET_FROM_PLAYER.x + vx,
                OFFSET_FROM_PLAYER.y
            );

        MoveVertical();
    }
    public override void OnExit() {
    }
}
