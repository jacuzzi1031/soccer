using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateRecovering: PlayerSimState
{
    private const int RECOVERY_TICKS = 30;
    private int recoveryTickCounter;
    public override void OnEnter()
    {
        recoveryTickCounter = 0;
    }
    public override void _Update(float deltaTime)
    {
        recoveryTickCounter++;

        if (recoveryTickCounter >= RECOVERY_TICKS)
        {
            playerSim.SwitchState(PlayerState.MOVING);
        }
    }
}
