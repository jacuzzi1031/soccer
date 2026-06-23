using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class PlayerStateCelebrating: PlayerSimState {
    private int initialDelayFrames;

    private const int CELEBRATING_HEIGHT = 50;

    public override void OnEnter() {
        int r = HashRandom(playerSim.playerId);
        //test for checksum dismatch
        // int r = HashRandom(playerSim.playerId+1);

        // 0.2s = 12帧, 0.3s = 18帧
        initialDelayFrames = 12 + (r % 19); // 0~18 → 12~30

        stateFrame = 0;
    }
    public override void _Update() {
        stateFrame++;

        if (playerSim.Height == 0 && stateFrame > initialDelayFrames) {
            Celebrate();
        }

        MoveHorizontal( GROUND_FRICTION);
    }



    private void Celebrate()
    {
        playerSim.Height = (FixedFloat)0.1f;
        playerSim.HeightVelocity = CELEBRATING_HEIGHT;
    }

    public override void OnExit() {
        playerSim.Height = FixedFloat.Zero;
        playerSim.HeightVelocity= FixedFloat.Zero;
    }


    public override void OnTeamReset(bool isHomeKickoff) {
        playerSim.SwitchState(PlayerState.RESETING,PlayerStateData.Build().SetResetPosition(isHomeKickoff?playerSim.kickoffPosition:playerSim.spawnPosition));

    }
}
