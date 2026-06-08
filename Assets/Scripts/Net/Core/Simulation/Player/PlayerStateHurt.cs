using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateHurt: PlayerSimState
{
    private const float AIR_FRICTION = 25f;
    private const float HURT_HEIGHT_VELOCITY = 50f;
    private const float BALL_TUMBLE_SPEED = 20f;
    private float _elapsedTicks;
    private float DURATION_HURT=1f;
    public override void OnEnter() {
        playerSim.HeightVelocity = HURT_HEIGHT_VELOCITY;
        playerSim.Height = 0.1f;
        Vector2 tumbleDir = stateData.MoveDir;
        _ballSim.Tumble(tumbleDir * BALL_TUMBLE_SPEED);
    }

    public override void _Update(float deltaTime) {
        MoveHorizontal(deltaTime,AIR_FRICTION);
        _elapsedTicks+=deltaTime;
        if (_elapsedTicks >= DURATION_HURT) {
            playerSim.SwitchState(PlayerState.RECOVERING);
        }
    }
}
