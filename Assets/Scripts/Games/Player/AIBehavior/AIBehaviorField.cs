using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class AIBehaviorField : AIBehavior
{
    private static FixedFloat PASS_PROBABILITY = (FixedFloat)0.05f;
    private static FixedFloat  SHOT_DISTANCE = (FixedFloat)150f;
    private static FixedFloat SHOT_PROBABILITY = (FixedFloat)0.3f;
    private static FixedFloat SPREAD_ASSIST_FACTOR = (FixedFloat)0.8f;
    private static FixedFloat TACKLE_DISTANCE = (FixedFloat)15f;
    private static FixedFloat TACKLE_PROBABILITY = (FixedFloat)0.3f;

    private static FixedFloat GoalYOffset     = (FixedFloat)10f;
    private static FixedFloat GoalYRangeSlack = (FixedFloat)10f;
    private static FixedFloat ReboundOffsetX  = (FixedFloat)100f;
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
                moveDir += GetCarrierReboundShotForce();
            }
            else {
                //队友推进
                moveDir += GetAssistFormationSteeringForce();
            }
        }
        //对手持球或者无人持球
        else
        {
            //依次上抢
            moveDir += GetOnDutySteeringForce();
    

                if (IsBallPossessedByOpponent())
                {
                    //上抢同时维持阵型
                    moveDir += GetSpawnSteeringForce();
                }
                else if (ballSim.carrier == null)
                {
                    ////无人持球范围内全力上抢
                    moveDir += GetBallProximitySteeringForce();
                    
                    moveDir += GetDensityAroundBallSteeringForce();
                }

        }
        moveDir = FixedVector2.ClampMagnitude(moveDir, FixedFloat.One);
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
    
    FixedVector2 GetOnDutySteeringForce()
    {
        return playerSim.weightOnDutySteering *
               (ballSim.Position - playerSim.Position).normalized;
    }
    
    FixedVector2 GetCarrierSteeringForce()
    {
        FixedVector2 target = playerSim.GetCenterTargetPosition();
        var playerPos = playerSim.Position;
        FixedVector2 direction = (target - playerPos).normalized;
        var v = target - playerPos;

        Debug.Log($"sqr={v.sqrMagnitude}");
        Debug.Log($"mag={v.magnitude}");

        var dir = v.normalized;

        Debug.Log($"dir normalized={dir}");
        Debug.Log($"dirMag={dir.magnitude}");
        FixedFloat weight = GetBiCircularWeight(playerPos, target, 30, (FixedFloat)0.2f, 150, 1);
        return direction * weight;
    }
    FixedVector2 GetCarrierReboundShotForce()
    {

    
        FixedVector2 carrierPos = ballSim.carrier.Position;
        FixedVector2 playerPos  = playerSim.Position;
    
        FixedVector2 topPos    =playerSim.GetTopTargetPosition();
        FixedVector2 bottomPos = playerSim.GetBottomTargetPosition();
        FixedVector2 centerPos = playerSim.GetCenterTargetPosition();
    
        FixedVector2 target = centerPos;
    
        bool outOfGoalYRange =
            carrierPos.y > topPos.y + GoalYRangeSlack ||
            carrierPos.y < bottomPos.y - GoalYRangeSlack;
    
        if (!outOfGoalYRange)
        {
            FixedFloat distToTop    = FixedFloat.Abs(carrierPos.y - topPos.y);
            FixedFloat distToBottom = FixedFloat.Abs(carrierPos.y - bottomPos.y);
    
            target.y = distToTop > distToBottom
                ? topPos.y + GoalYOffset
                : bottomPos.y - GoalYOffset;
        }
    
        target.x = centerPos.x - FixedFloat.Sign(centerPos.x) * ReboundOffsetX;
    
        FixedVector2 direction = (target - playerPos).normalized;
    
        FixedFloat weight = GetBiCircularWeight(
            playerPos,
            target,
            1,
            0,
            20,
            1
        );
    
        return direction * weight;
    }
    
    
    FixedVector2 GetAssistFormationSteeringForce()
    {
        FixedVector2 spawnDiff = ballSim.spawnPosition - playerSim.spawnPosition;
        FixedVector2 destination = ballSim.Position - spawnDiff * SPREAD_ASSIST_FACTOR;
    
        FixedVector2 direction = (destination - playerSim.Position).normalized;
        FixedFloat weight = GetBiCircularWeight(playerSim.Position, destination, 60, (FixedFloat)0.2f, (FixedFloat)120, (FixedFloat)1);
    
        return direction * weight;
    }
    //无人持球全力抢球
    FixedVector2 GetBallProximitySteeringForce()
    {
        var playerPos = playerSim.Position;
        var carrierPos = ballSim.Position;
        FixedFloat weight = GetBiCircularWeight(playerPos, carrierPos, 200, 1, 500, (FixedFloat)0.2f);
        FixedVector2 direction = (carrierPos - playerPos).normalized;
    
        return direction * weight;
    }
    //防守维持阵型
    FixedVector2 GetSpawnSteeringForce() {
        var playerPos = playerSim.Position;
        var spawnPos = playerSim.spawnPosition;
        FixedFloat weight = GetBiCircularWeight(playerPos, spawnPos, 45, 0, 150, 1);
        FixedVector2 direction = (spawnPos - playerPos).normalized;
        return direction * weight;
    }
    //一个队不一拥而上
    FixedVector2 GetDensityAroundBallSteeringForce()
    {
        int count = playerSim.isHome?homeCount:awayCount;
        if (count == 0) return FixedVector2.Zero;
    
        FixedFloat weight =(FixedFloat)( 1 - (1f / count));
        FixedVector2 direction = (playerSim.Position - ballSim.Position).normalized;
    
        return direction * weight;
    }
    
    bool IsBallCarriedByTeammate()
    {
        return ballSim.carrier != null && ballSim.carrier.isHome == playerSim.isHome;
    }
    
    bool IsBallPossessedByOpponent()
    {
        return ballSim.carrier != null && ballSim.carrier.isHome != playerSim.isHome;
    }
    
    bool HasOpponentsNearby()
    {
        int count=playerSim.isHome?awayCount:homeCount;
        return count>0;
    }
    
    // UTILS: Bicircular Weight
    FixedFloat GetBiCircularWeight(FixedVector2 pos, FixedVector2 target, FixedFloat r1, FixedFloat w1, FixedFloat r2, FixedFloat w2)
    {
        FixedFloat d = FixedVector2.Distance(pos, target);
    
        if (d < r1) return w1;
        if (d > r2) return w2;
    
        FixedFloat t = (d - r1) / (r2 - r1);
        return FixedMath.Lerp(w1, w2, t);
    }
    
    private bool TeammateIntoShootingScope() {
        FixedVector2 target = playerSim.GetCenterTargetPosition();
        return FixedVector2.Distance(ballSim.carrier.Position, target) < SHOT_DISTANCE;
    }
}
