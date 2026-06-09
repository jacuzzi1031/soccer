using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateCelebrating: PlayerSimState {
    private float startCelebratingTime;

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
        MoveHorizontal(deltaTime,AIR_FRICTION);
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


    public override void OnTeamReset(bool isHomeKickoff) {
        playerSim.SwitchState(PlayerState.RESETING,PlayerStateData.Build().SetResetPosition(isHomeKickoff?playerSim.kickoffPosition:playerSim.spawnPosition));

    }
}
