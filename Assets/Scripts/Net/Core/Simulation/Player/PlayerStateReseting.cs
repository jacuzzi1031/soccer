using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class PlayerStateReseting : PlayerSimState{
    public bool hasArrived = false;
    public FixedFloat arriveDistance = (FixedFloat)1f;

    public override void OnEnter() {
        hasArrived = false;
    }

    public override void _Update() {
        if (hasArrived) return;
        FixedVector2 playerPosition = playerSim.Position;
        FixedVector2 dir = (stateData.ResetPosition - playerPosition).normalized;
        if ((stateData.ResetPosition - playerPosition).sqrMagnitude < arriveDistance * arriveDistance) {
            hasArrived = true;
            playerSim.HeadingRight = playerSim.initialFacingRight;
            playerSim.Velocity = FixedVector2.Zero;
        }
        else {
            playerSim.Velocity = dir * playerSim.Speed;
            playerSim.SetHeadingRight(dir);
        }

        playerPosition += playerSim.Velocity * SimulationConfig.DeltaTime;
        playerSim.Position = playerPosition;
    }

    public override bool IsReadyForKickoff() {
        return hasArrived;
    }

    public override bool CanCarryBall() {
        return hasArrived;
    }
}