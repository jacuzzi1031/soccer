using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMoving: PlayerSimState
{   
    private Vector2 moveDir;
    private float goalieSpeed = 44f;
    public override void OnEnter() {
    }

    public override void _Update(float deltaTime) {
        if (playerSim.controlScheme == ControlScheme.CPU) {
            // aiBehavior.UpdateAI();                  
            // moveDir = aiBehavior.GetAIMoveDir();  
        }
        else {
            moveDir = _moveDirection;
        }
        
        float speed = playerSim.role != Role.GOALIE
            ? playerSim.Speed
            : goalieSpeed;

        playerSim.Velocity = moveDir * speed;

        Vector2 nextPos = playerSim.Position + playerSim.Velocity * deltaTime;

        playerSim.Position = nextPos;
        playerSim.SetHeadingRight(moveDir);
    }

    public override void OnShootPress(bool isInstant,bool hasBall,bool ballCanAirInteract) {
        if (hasBall)
        {
            playerSim.SwitchState(PlayerState.PREPPING_SHOT);
        }
        else if (ballCanAirInteract) {
            InstantShot();
        }
    }
    public override void OnShootRelease(bool hasBall,bool ballCanAirInteract) {
        if (!hasBall&&ballCanAirInteract) {
            InstantShot();
        }
        PlayerStateData data = PlayerStateData.Build()
            .SetShotPower(playerSim.Power)
            .SetShotDirection(_moveDirection)
            .SetIsInstant(true);
        playerSim.SwitchState(PlayerState.SHOOTING, data);
    }
    private void InstantShot() {
        if (playerSim.IsFacingTargetGoal())
        {
            if (_ballSim.height <= 4.3f) {
                playerSim.SwitchState(PlayerState.VOLLEY_KICK);
            }
            else {
                playerSim.SwitchState(PlayerState.HEADER);
            }

        }
        else
        {
            playerSim.SwitchState(PlayerState.BICYCLE_KICK);
        }
    }
    public override void OnPass(int passType,PlayerSim passTarget) {
        bool hasBall = playerSim._ballSim.BallCarrierId == playerSim.playerId;
        if (!hasBall) {//tackle
            return;
        }
        playerSim.SwitchState(PlayerState.PASSING,PlayerStateData.Build().SetInputType(passType).SetMoveDir(moveDir).setPassTarget(passTarget));
    }

    public override bool CanCarryBall() {
        return playerSim.role != Role.GOALIE;
    }
}
