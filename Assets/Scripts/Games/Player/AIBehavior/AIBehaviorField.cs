using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBehaviorField : AIBehavior
{
private const float PASS_PROBABILITY = 0.05f;
    private const float SHOT_DISTANCE = 150f;
    private const float SHOT_PROBABILITY = 0.3f;
    private const float SPREAD_ASSIST_FACTOR = 0.8f;
    private const float TACKLE_DISTANCE = 15f;
    private const float TACKLE_PROBABILITY = 0.3f;


    public override void PerformAIMovement()
    {
        moveDir = Vector2.zero;
    
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
    
            if (moveDir.sqrMagnitude < 0.95f)
            {
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
        }
    
        moveDir = Vector2.ClampMagnitude(moveDir, 1f);
    }
    

    
    public override void PerformAIDecisions() {
        var ballPos = ballSim.Position;
        var playerPos = playerSim.Position;
        if (IsBallPossessedByOpponent() &&
            Vector2.Distance(playerPos, ballPos) < TACKLE_DISTANCE &&
            Random.value < TACKLE_PROBABILITY)
        {
            playerSim.SwitchState(PlayerState.TACKLING);
        }
        //玩家控制的队伍不会射门传球
        if (playerSim.isHome || !playerSim.isHome && matchPlayerCount > 1) {
            return;
        }
        
        if (ballSim.carrier?.playerId == playerSim.playerId)
        {
            Vector2 target = playerSim.GetCenterTargetPosition();
            
            
            if (Vector2.Distance(playerPos, target) < SHOT_DISTANCE &&
                Random.value < SHOT_PROBABILITY)
            {
                Vector2 direction = (playerSim.GetFarTargetPosition() - playerPos).normalized;
    
                var data = new PlayerStateData()
                    .SetShotPower(playerSim.Power)
                    .SetShotDirection(direction);
    
                playerSim.SwitchState(PlayerState.SHOOTING, data);
            }
            else if (Random.value < PASS_PROBABILITY &&
                     HasOpponentsNearby()
                     )
            {
                //汤球
                playerSim.SwitchState(PlayerState.PASSING);
                
                //或者回传
            }
        }
    }
    
    Vector2 GetOnDutySteeringForce()
    {
        return playerSim.weightOnDutySteering *
               (ballSim.Position - playerSim.Position).normalized;
    }
    
    Vector2 GetCarrierSteeringForce()
    {
        Vector2 target = playerSim.GetCenterTargetPosition();
        var playerPos = playerSim.Position;
        Vector2 direction = (target - playerPos).normalized;
    
        float weight = GetBiCircularWeight(playerPos, target, 100, 0, 150, 1);
        return direction * weight;
    }
    Vector2 GetCarrierReboundShotForce()
    {
        const float GoalYOffset     = 10f;
        const float GoalYRangeSlack = 10f;
        const float ReboundOffsetX  = 100f;
    
        Vector2 carrierPos = ballSim.carrier.Position;
        Vector2 playerPos  = playerSim.Position;
    
        Vector2 topPos    =playerSim.GetTopTargetPosition();
        Vector2 bottomPos = playerSim.GetBottomTargetPosition();
        Vector2 centerPos = playerSim.GetCenterTargetPosition();
    
        Vector2 target = centerPos;
    
        bool outOfGoalYRange =
            carrierPos.y > topPos.y + GoalYRangeSlack ||
            carrierPos.y < bottomPos.y - GoalYRangeSlack;
    
        if (!outOfGoalYRange)
        {
            float distToTop    = Mathf.Abs(carrierPos.y - topPos.y);
            float distToBottom = Mathf.Abs(carrierPos.y - bottomPos.y);
    
            target.y = distToTop > distToBottom
                ? topPos.y + GoalYOffset
                : bottomPos.y - GoalYOffset;
        }
    
        target.x = centerPos.x - Mathf.Sign(centerPos.x) * ReboundOffsetX;
    
        Vector2 direction = (target - playerPos).normalized;
    
        float weight = GetBiCircularWeight(
            playerPos,
            target,
            1,
            0,
            20,
            1
        );
    
        return direction * weight;
    }
    
    
    Vector2 GetAssistFormationSteeringForce()
    {
        Vector2 spawnDiff = ballSim.spawnPosition - playerSim.spawnPosition;
        Vector2 destination = ballSim.Position - spawnDiff * SPREAD_ASSIST_FACTOR;
    
        Vector2 direction = (destination - playerSim.Position).normalized;
        float weight = GetBiCircularWeight(playerSim.Position, destination, 60, 0.2f, 120, 1);
    
        return direction * weight;
    }
    //无人持球全力抢球
    Vector2 GetBallProximitySteeringForce()
    {
        var playerPos = playerSim.Position;
        var carrierPos = ballSim.Position;
        float weight = GetBiCircularWeight(playerPos, carrierPos, 50, 1, 120, 0);
        Vector2 direction = (carrierPos - playerPos).normalized;
    
        return direction * weight;
    }
    //防守维持阵型
    Vector2 GetSpawnSteeringForce() {
        var playerPos = playerSim.Position;
        var spawnPos = playerSim.spawnPosition;
        float weight = GetBiCircularWeight(playerPos, spawnPos, 30, 0, 100, 1);
        Vector2 direction = (spawnPos - playerPos).normalized;
        return direction * weight;
    }
    //一个队不一拥而上
    Vector2 GetDensityAroundBallSteeringForce()
    {
        int count = playerSim.isHome?homeCount:awayCount;
        if (count == 0) return Vector2.zero;
    
        float weight = (count - 1) * 0.8f;
        Vector2 direction = (playerSim.Position - ballSim.Position).normalized;
    
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
    float GetBiCircularWeight(Vector2 pos, Vector2 target, float r1, float w1, float r2, float w2)
    {
        float d = Vector2.Distance(pos, target);
    
        if (d < r1) return w1;
        if (d > r2) return w2;
    
        float t = (d - r1) / (r2 - r1);
        return Mathf.Lerp(w1, w2, t);
    }
    
    private bool TeammateIntoShootingScope() {
        Vector2 target = playerSim.GetCenterTargetPosition();
        return Vector2.Distance(ballSim.carrier.Position, target) < SHOT_DISTANCE;
    }
}
