using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class AIBehaviorGoalie : AIBehavior
{
    private static FixedFloat PROXIMITY_CONCERN = (FixedFloat)10f;
    private static FixedFloat EXPAND_GOAL = (FixedFloat)12f;
    public override void PerformAIDecisions() {
        if (IsBallHeadingToGoal(ballSim.Position, ballSim.Velocity, GoalArea))
        {
            playerSim.SwitchState(PlayerState.DIVING);
        }
    }

    public override void PerformAIMovement() {
         moveDir = GetGoalieSteeringForce();
        
        if (moveDir.magnitude > FixedFloat.One)
            moveDir = moveDir.normalized;
    }
    public bool IsBallHeadingToGoal(
        FixedVector2 ballPos,
        FixedVector2 ballVel,
        FixedRect goalArea)
    {
        if (ballVel.sqrMagnitude < FixedFloat.Zero)
            return false;
        FixedVector2 dir = ballVel.normalized;

        FixedFloat maxDistance = ballVel.magnitude * FixedFloat.One;

        FixedRect expandedGoal = goalArea;
        expandedGoal.yMin -= EXPAND_GOAL;
        expandedGoal.yMax += EXPAND_GOAL;
        FixedFloat hitTime;
        return DeterministicGeometry2D.RayIntersectsAABB(
            ballPos,
            dir,
            expandedGoal,
            maxDistance,
            out hitTime
        );
    }

    public FixedVector2 GetGoalieSteeringForce()
    {
        FixedVector2 top = playerSim.GetTopTargetPosition();
        FixedVector2 bottom = playerSim.GetBottomTargetPosition();

        FixedFloat minY = FixedMath.Min(top.y, bottom.y);
        FixedFloat maxY = FixedMath.Max(top.y, bottom.y);

        FixedFloat targetY = FixedFloat.Clamp(ballSim.Position.y, minY, maxY);
        FixedFloat distanceY = targetY - playerSim.Position.y;
        FixedFloat weight = FixedFloat.Clamp01(FixedFloat.Abs(distanceY) / PROXIMITY_CONCERN);

        FixedVector2 direction =new FixedVector2(FixedFloat.Zero, FixedFloat.Sign(distanceY));

        return direction * weight;
    }
}
