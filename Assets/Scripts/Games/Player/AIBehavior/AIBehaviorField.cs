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
    private const float TACKLE_PROBABILITY = 0.15f;

    void Update()
    {
        PerformAIMovement();
        PerformAIDecisions();
    }
    public override void PerformAIMovement()
    {
        moveDir = Vector2.zero;

        if (player.HasBall())
        {
            moveDir += GetCarrierSteeringForce();
        }
        else if (IsBallCarriedByTeammate())
        {
            moveDir += GetAssistFormationSteeringForce();
        }
        else
        {
            moveDir += GetOnDutySteeringForce();

            if (moveDir.sqrMagnitude < 1f)
            {
                if (IsBallPossessedByOpponent())
                {
                    moveDir += GetSpawnSteeringForce();
                }
                else if (ball.carrier == null)
                {
                    moveDir += GetBallProximitySteeringForce();
                    moveDir += GetDensityAroundBallSteeringForce();
                }
            }
        }

        moveDir = Vector2.ClampMagnitude(moveDir, 1f);
    }
    
    public override void PerformAIDecisions()
    {
        if (IsBallPossessedByOpponent() &&
            Vector2.Distance(player.transform.position, ball.transform.position) < TACKLE_DISTANCE &&
            Random.value < TACKLE_PROBABILITY)
        {
            player.SwitchState(Player.State.TACKLING);
        }
        //当cpu控制为玩家所在队伍，不进行shot/pass
        if (GameManager.Instance.playerSetup[0] == player.country)
            return;

        if (ball.carrier == player)
        {
            Vector2 target = player.targetGoal.GetCenterTargetPosition();
            
            
            if (Vector2.Distance(player.transform.position, target) < SHOT_DISTANCE &&
                Random.value < SHOT_PROBABILITY)
            {
                player.FaceTowardsTargetGoal();
                Vector2 direction = (player.targetGoal.GetRandomTargetPosition() - (Vector2)player.transform.position).normalized;

                var data = new PlayerStateData()
                    .SetShotPower(player.power)
                    .SetShotDirection(direction);

                player.SwitchState(Player.State.SHOOTING, data);
            }
            else if (Random.value < PASS_PROBABILITY &&
                     HasOpponentsNearby()
                     )
            {
                player.SwitchState(Player.State.PASSING);
            }
        }
    }

    // ------------------------------
    // STEERING FORCE FUNCTIONS
    // ------------------------------
    Vector2 GetOnDutySteeringForce()
    {
        return player.weightOnDutySteering *
               (ball.transform.position - player.transform.position).normalized;
    }

    Vector2 GetCarrierSteeringForce()
    {
        Vector2 target = player.targetGoal.GetCenterTargetPosition();
        Vector2 direction = (target - (Vector2)player.transform.position).normalized;

        float weight = GetBiCircularWeight(player.transform.position, target, 100, 0, 150, 1);
        return direction * weight;
    }

    Vector2 GetAssistFormationSteeringForce()
    {
        Vector2 spawnDiff = ball.carrier.spawnPosition - player.spawnPosition;
        Vector2 destination = (Vector2)ball.carrier.transform.position - spawnDiff * SPREAD_ASSIST_FACTOR;

        Vector2 direction = (destination - (Vector2)player.transform.position).normalized;
        float weight = GetBiCircularWeight(player.transform.position, destination, 30, 0.2f, 60, 1);

        return direction * weight;
    }
    //足球没有被carried全力抢球
    Vector2 GetBallProximitySteeringForce()
    {
        float weight = GetBiCircularWeight(player.transform.position, ball.transform.position, 50, 1, 120, 0);
        Vector2 direction = (ball.transform.position - player.transform.position).normalized;

        return direction * weight;
    }
    //防守维持阵型
    Vector2 GetSpawnSteeringForce()
    {
        float weight = GetBiCircularWeight(player.transform.position, player.spawnPosition, 30, 0, 100, 1);
        Vector2 direction = (player.spawnPosition - (Vector2)player.transform.position).normalized;

        return direction * weight;
    }
    //一个队不一拥而上
    Vector2 GetDensityAroundBallSteeringForce()
    {
        int count = ball.GetProximityTeammatesCount(player.country);
        if (count == 0) return Vector2.zero;

        float weight = 1 - (1f / count);
        Vector2 direction = (player.transform.position - ball.transform.position).normalized;

        return direction * weight;
    }

    bool IsBallCarriedByTeammate()
    {
        return ball.carrier != null && ball.carrier.country == player.country;
    }

    bool IsBallPossessedByOpponent()
    {
        return ball.carrier != null && ball.carrier.country != player.country;
    }

    bool HasOpponentsNearby()
    {
        return player.HasOpponentsNearby();
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
}
