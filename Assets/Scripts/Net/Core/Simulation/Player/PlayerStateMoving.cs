using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class PlayerStateMoving: PlayerSimState
{   
    private FixedVector2 moveDir;
    private FixedFloat goalieSpeed = (FixedFloat)34f;
    private bool couldCarrer;
    private int _elapsedFrames;
    private int couldCarryFrames = 24;
    public override void OnEnter() {
        _elapsedFrames = 0;
        couldCarrer = false;
    }

    public override void _Update() {
        if (!couldCarrer && _elapsedFrames < couldCarryFrames) {
            _elapsedFrames++;
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
        
        FixedFloat speed = playerSim.role != Role.GOALIE
            ? playerSim.Speed
            : goalieSpeed;

        playerSim.Velocity = moveDir * speed;

        FixedVector2 nextPos = playerSim.Position + playerSim.Velocity * SimulationConfig.DeltaTime;

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
            if (_ballSim.height <= (FixedFloat)0.3f) {
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
    public override void OnPass(FixedVector2 Direction,int passType,PlayerSim passTarget) {
        playerSim.SwitchState(PlayerState.PASSING,PlayerStateData.Build().SetInputType(passType).SetMoveDir(Direction).setPassTarget(passTarget));
    }

    public override void OnTackle(FixedVector2 direction) {
        playerSim.SwitchState(PlayerState.TACKLING,PlayerStateData.Build().SetMoveDir(Direction));

    }

    public override bool CanCarryBall() {
        return playerSim.role != Role.GOALIE&&couldCarrer;
    }

    public override bool CouldHurt() {
        if (_ballSim.carrier?.playerId == playerSim.playerId) {
            return true;
        }
        return false;
    }
}
