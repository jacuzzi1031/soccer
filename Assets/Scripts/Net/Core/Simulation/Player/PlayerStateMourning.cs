using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMourning: PlayerSimState
{
    public override void OnEnter() {
        playerSim.Velocity=Vector2.zero;
    }

    public override void OnExit() {
        if (playerSim.playerId == 0) {
        } 
    }

    public override void OnTeamReset(bool isHomeKickoff) {
        playerSim.SwitchState(PlayerState.RESETING,PlayerStateData.Build().SetResetPosition(isHomeKickoff?playerSim.kickoffPosition:playerSim.spawnPosition));
    }
}
