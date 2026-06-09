using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBehaviorGoalie : AIBehavior
{
    private const float PROXIMITY_CONCERN = 10f;
    public override void PerformAIDecisions() {
        if (IsBallHeadingToGoal(ballSim.Position, ballSim.Velocity, GoalArea))
        {
            playerSim.SwitchState(PlayerState.DIVING);
        }
    }

    public override void PerformAIMovement() {
         moveDir = GetGoalieSteeringForce();
        
        if (moveDir.magnitude > 1f)
            moveDir = moveDir.normalized;
    }
    public bool IsBallHeadingToGoal(
        Vector2 ballPos,
        Vector2 ballVel,
        Rect goalArea)
    {
        if (ballVel.sqrMagnitude < 0.0001f)
            return false;
        Vector2 dir = ballVel.normalized;

        float maxDistance = ballVel.magnitude * 1.0f;
        // 只扩大Y范围
        Rect expandedGoal = goalArea;
        expandedGoal.yMin -= 12.0f;
        expandedGoal.yMax += 12.0f;
        float hitTime;
        return DeterministicGeometry2D.RayIntersectsAABB(
            ballPos,
            dir,
            expandedGoal,
            maxDistance,
            out hitTime
        );
    }

    public Vector2 GetGoalieSteeringForce()
    {
        Vector2 top = playerSim.GetTopTargetPosition();
        Vector2 bottom = playerSim.GetBottomTargetPosition();

        float minY = Mathf.Min(top.y, bottom.y);
        float maxY = Mathf.Max(top.y, bottom.y);

        float targetY = Mathf.Clamp(ballSim.Position.y, minY, maxY);
        float distanceY = targetY - playerSim.Position.y;
        float weight = Mathf.Clamp01(Mathf.Abs(distanceY) / PROXIMITY_CONCERN);

        Vector2 direction = new Vector2(0f, Mathf.Sign(distanceY));

        return direction * weight;
    }
}
