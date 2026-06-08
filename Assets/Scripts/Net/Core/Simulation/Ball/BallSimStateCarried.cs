using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSimStateCarried : BallSimState
{
    public override void OnEnter() {
        ballSim.Velocity=Vector2.zero;
        ballSim.heightVelocity=0.0f;
        if (ballSim.height > 0.6f) {
            ballSim.height -= 0.2f;
        }
    }
    private float dribbleTime = 0f;
    private const float DRIBBLE_FREQUENCY = 10f;
    private const float DRIBBLE_INTENSITY = 3f;
    private readonly Vector2 OFFSET_FROM_PLAYER = new Vector2(10.8f, 4f);
    public override void _Update(float deltaTime) {
        dribbleTime += deltaTime;
        float vx = 0f;
        if (ballSim.carrier.Velocity.x != 0)
            vx = Mathf.Cos(dribbleTime * DRIBBLE_FREQUENCY) * DRIBBLE_INTENSITY;
        bool facingRight = ballSim.carrier.HeadingRight;
        ballSim.Position =
            ballSim.carrier.Position +
            new Vector2(facingRight ? OFFSET_FROM_PLAYER.x + vx : -OFFSET_FROM_PLAYER.x + vx, OFFSET_FROM_PLAYER.y);
        MoveVertical(deltaTime);
    }
    public override void OnExit() {
    }
}
