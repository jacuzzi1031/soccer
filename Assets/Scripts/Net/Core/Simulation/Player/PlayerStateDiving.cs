using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateDiving: PlayerSimState
{
    public float _durationTicks = 0.5f;
    private float _elapsedTicks;
    public float timeStartDive;
    public override void OnEnter() {
        Vector2 targetDive = new Vector2(playerSim.spawnPosition.x, _ballSim.Position.y);
        diveDir = (targetDive - playerSim.Position).normalized;

        _elapsedTicks = 0f;
    }

    public override void _Update(float deltaTime) {
        _elapsedTicks+=deltaTime;

        if (_elapsedTicks >= _durationTicks)
        {
            playerSim.SwitchState(PlayerState.RECOVERING);
        }
    }
}
