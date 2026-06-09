using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerStateDiving: PlayerSimState
{
    public float _durationTicks = 0.5f;
    private float _elapsedTicks;
    public float timeStartDive;
    public override void OnEnter()
    {
        float goalLineX = playerSim.spawnPosition.x;

        Vector2 ballPos = _ballSim.Position;
        Vector2 ballVel = _ballSim.Velocity;

        Vector2 targetDive;

        if (Mathf.Abs(ballVel.x) > 0.01f)
        {
            float t = (goalLineX - ballPos.x) / ballVel.x;
            if (t > 0)
            {
                float impactY = ballPos.y + ballVel.y * t;
                targetDive = new Vector2(goalLineX, impactY);
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

        Vector2 moveDir =
            (targetDive - playerSim.Position).normalized;

        playerSim.Velocity =
            moveDir * playerSim.Speed;
        diveDirY = playerSim.Velocity.y;
        _elapsedTicks = 0f;
    }

    public override void _Update(float deltaTime) {
        _elapsedTicks+=deltaTime;
        if (math.abs(playerSim.Velocity.y) < 0.1f) {
            playerSim.Velocity = new Vector2(0f, diveDirY*1.1f);
        }
        playerSim.Position += deltaTime * playerSim.Velocity;
        if (_elapsedTicks >= _durationTicks)
        {
            playerSim.SwitchState(PlayerState.RECOVERING);
        }
    }
}
