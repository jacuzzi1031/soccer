using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateCelebrating: PlayerSimState {
    private float startCelebratingTime;
    private const float AIR_FRICTION = 60f;
    private float initialDelay;
    private const float CELEBRATING_HEIGHT = 50f;
    public override void OnEnter() {
        float r = HashRandom(playerSim.playerId);
        initialDelay = 0.2f + r * 0.3f;
        startCelebratingTime = 0f;
    }
    public override void _Update(float deltaTime) {
        startCelebratingTime+= deltaTime;
        if (playerSim.Height == 0f && startCelebratingTime > initialDelay)
        {
            Celebrate();
        }
        MoveHorizontal(deltaTime);
    }

    private void MoveHorizontal(float deltaTime) {
        Vector2 velocity = playerSim.Velocity;
        Vector2 position = playerSim.Position;
        
        velocity = Vector2.MoveTowards(
            velocity,
            Vector2.zero,
            AIR_FRICTION * deltaTime
        );
        if (velocity.sqrMagnitude < 0.0001f)
        {
            playerSim.Velocity = Vector2.zero;
            return;
        }

        Vector2 move = velocity * deltaTime;

        position += move;

        playerSim.Velocity = velocity;
        playerSim.Position = position;
    }

    private void Celebrate()
    {
        playerSim.Height = 0.1f;
        playerSim.HeightVelocity = CELEBRATING_HEIGHT;
    }

    public override void OnExit() {
        playerSim.Height = 0.0f;
        playerSim.HeightVelocity=0f;
    }
    float HashRandom(int seed)
    {
        uint x = (uint)seed;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;

        return (x % 1000) / 1000f;
    }

    public override void OnTeamReset(bool isHomeKickoff) {
        playerSim.SwitchState(PlayerState.RESETING,PlayerStateData.Build().SetResetPosition(isHomeKickoff?playerSim.kickoffPosition:playerSim.teamResetPosition));

    }
}
