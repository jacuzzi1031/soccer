using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMoving: PlayerSimState
{   
    private Vector2 moveDir;
    private float goalieSpeed = 34f;
    private bool couldCarrer;
    private float _elapsedTicks;
    private float couldCarryTicks=0.4f;
    public override void OnEnter() {
        _elapsedTicks = 0f;
        couldCarrer = false;
    }

    public override void _Update(float deltaTime) {
        if (!couldCarrer&&_elapsedTicks<couldCarryTicks) {
            _elapsedTicks += deltaTime;
        }
        else {
            couldCarrer = true;
        }
        
        
        if (playerSim.controlScheme == ControlScheme.CPU) {
            playerSim.aiBehavior.UpdateAI();
            moveDir = playerSim.aiBehavior.GetAIMoveDir();
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
            if (_ballSim.height <= 3.3f) {
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
    public override void OnPass(Vector2 Direction,int passType,PlayerSim passTarget) {
        playerSim.SwitchState(PlayerState.PASSING,PlayerStateData.Build().SetInputType(passType).SetMoveDir(Direction).setPassTarget(passTarget));
    }

    public override void OnTackle(Vector2 direction) {
        playerSim.SwitchState(PlayerState.TACKLING,PlayerStateData.Build().SetMoveDir(Direction));

    }

    public override bool CanCarryBall() {
        return playerSim.role != Role.GOALIE&&couldCarrer;
    }

    public override bool CouldHurt() {
        if (_ballSim.carrier.playerId == playerSim.playerId) {
            return true;
        }
        return false;
    }
}
