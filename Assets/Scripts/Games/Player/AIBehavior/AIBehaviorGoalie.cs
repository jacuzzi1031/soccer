using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBehaviorGoalie : AIBehavior
{
    private const float PROXIMITY_CONCERN = 10f;
    public override void PerformAIDecisions() {
        if (BallView.IsHeadedForScoringArea(PlayerView.ownGoal.GetScoringArea()))
        {
            PlayerView.SwitchViewState(PlayerView.State.DIVING);
        }
    }

    public override void PerformAIMovement() {
         moveDir = GetGoalieSteeringForce();
        
        if (moveDir.magnitude > 1f)
            moveDir = moveDir.normalized;
    }


    public Vector2 GetGoalieSteeringForce()
    {
        Vector2 top = PlayerView.ownGoal.GetTopTargetPosition();
        Vector2 bottom = PlayerView.ownGoal.GetBottomTargetPosition();

        float minY = Mathf.Min(top.y, bottom.y);
        float maxY = Mathf.Max(top.y, bottom.y);

        float targetY = Mathf.Clamp(BallView.transform.position.y, minY, maxY);
        float distanceY = targetY - PlayerView.transform.position.y;
        float weight = Mathf.Clamp01(Mathf.Abs(distanceY) / PROXIMITY_CONCERN);

        Vector2 direction = new Vector2(0f, Mathf.Sign(distanceY));

        return direction * weight;
    }
}
