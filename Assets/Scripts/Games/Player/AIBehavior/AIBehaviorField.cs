using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

namespace Games.Player.AIBehavior{
    public class AIBehaviorField : AIBehavior
{

    public override void PerformAIMovement()
    {
        moveDir = FixedVector2.Zero;
    
        if (playerSim.HasBall())
        {
            moveDir += GetCarrierSteeringForce();
        } 
        //队友持球
        else if (IsBallCarriedByTeammate())
        {
            //队友禁区跑位
            if (TeammateIntoShootingScope()) {
                if (playerSim.playerId > 3) {
                    moveDir += GetCarrierReboundShotForce();
                }
            }
            else {
                //队友推进
                moveDir += GetAssistFormationSteeringForce();
            }
        }
        //对手持球或者无人持球
        else
        {
            moveDir += GetOnDutySteeringForce(role);
            if (IsBallPossessedByOpponent()) {
                switch(playerSim.playerId)
                {
                    case 1:
                    case 2:
                       
                        moveDir += GetSpawnSteeringForce();
                        break;
                    case 3:
                        moveDir += GetAttackHoldForce(role);
                        break;
                    case 4:
                    case 5:
                        moveDir += GetAttackHoldForce(role);
                        break;
                }
            }
            else if (ballSim.carrier == null)
            {
                // ////无人持球范围内全力上抢
                // moveDir += GetBallProximitySteeringForce();
                //
                // moveDir += GetDensityAroundBallSteeringForce();
            }

        }
        moveDir = FixedVector2.ClampMagnitude(moveDir, FixedFloat.One);
    }
    protected FixedVector2 GetAttackHoldForce()
    {
        FixedVector2 target =
            GetAttackTarget(ReboundOffsetX);

        FixedVector2 dir =
            (target - playerSim.Position).normalized;

        return dir;
    }
    

    
    public override void PerformAIDecisions() {
        var ballPos = ballSim.Position;
        var playerPos = playerSim.Position;
        if (IsBallPossessedByOpponent() &&
            FixedVector2.Distance(playerPos, ballPos) < TACKLE_DISTANCE &&
            FixedMath.Roll(TACKLE_PROBABILITY))
        {
            playerSim.SwitchState(PlayerState.TACKLING);
        }
        //玩家控制的队伍不会射门传球
        if (playerSim.isHome || !playerSim.isHome && matchPlayerCount > 1) {
            return;
        }
        
        if (ballSim.carrier?.playerId == playerSim.playerId)
        {
            FixedVector2 target = playerSim.GetCenterTargetPosition();
            
            
            if (FixedVector2.Distance(playerPos, target) < SHOT_DISTANCE &&
                FixedMath.Roll(SHOT_PROBABILITY) )
            {
                FixedVector2 direction = (playerSim.GetFarTargetPosition() - playerPos).normalized;
    
                var data = new PlayerStateData()
                    .SetShotPower(playerSim.Power)
                    .SetShotDirection(direction);
    
                playerSim.SwitchState(PlayerState.SHOOTING, data);
            }
            else if ( FixedMath.Roll(PASS_PROBABILITY) && HasOpponentsNearby())
            {
                //汤球
                playerSim.SwitchState(PlayerState.PASSING);
                
                //或者回传
            }
        }
    }
    
    
    

    
   
    FixedVector2 GetCarrierReboundShotForce()
    {
        FixedVector2 target = GetAttackTarget(ReboundOffsetX);

        FixedVector2 direction =
            (target - playerSim.Position).normalized;

        FixedFloat weight = GetBiCircularWeight(
            playerSim.Position,
            target,
            1,
            0,
            20,
            1);

        return direction * weight;
    }
    
    
    FixedVector2 GetAssistFormationSteeringForce()
    {
        int playerId = playerSim.playerId;

        // 后场球员维持阵型
        if (playerId <= 2)
        {
            FixedVector2 spawnDiff = ballSim.spawnPosition - playerSim.spawnPosition;
            FixedVector2 destination = ballSim.Position - spawnDiff * SPREAD_ASSIST_FACTOR;
    
            FixedVector2 direction = (destination - playerSim.Position).normalized;
            FixedFloat weight = GetBiCircularWeight(playerSim.Position, destination, 30, (FixedFloat)0.2f, (FixedFloat)60, (FixedFloat)1);
    
            return direction * weight;
        }

        FixedVector2 attackTarget;

        // 中场跑弧顶
        if (playerId == 3)
        {
            attackTarget = GetAttackTarget(ReboundOffsetX*2);
        }
        // 前锋直接冲更深的位置
        else
        {
            attackTarget = GetAttackTarget(ReboundOffsetX );
        }

        FixedVector2 attackDir =
            (attackTarget - playerSim.Position).normalized;

        FixedFloat attackWeight = GetBiCircularWeight(
            playerSim.Position,
            attackTarget,
            100,
            0,
            150,
            1);
        return attackDir * attackWeight;
    }
    //无人持球全力抢球

    //防守维持阵型


    

    

    

    


}
}


