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

        ResolveBoundary(ref nextPos);

        playerSim.Position = nextPos;
    }

    private void ResolveBoundary(ref Vector2 position) {
        foreach (var line in playerSim.lines)
        {
            Vector2 closest = ClosestPointOnSegment(position, line);

            Vector2 diff = position - closest;
            float sqrDist = diff.sqrMagnitude;

            if (sqrDist < playerSim.radius * playerSim.radius)
            {
                if (sqrDist < 0.0000001f)
                    continue;

                float dist = Mathf.Sqrt(sqrDist);
                Vector2 normal = diff / dist;
                
                position = closest + normal * playerSim.radius;
            }
        }
    }
    private Vector2 ClosestPointOnSegment(Vector2 point, LineSegment line)
    {
        Vector2 ab = line.End - line.Start;
        float abSqr = Vector2.Dot(ab, ab);

        if (abSqr < 0.000001f)
            return line.Start;

        float t = Vector2.Dot(point - line.Start, ab) / abSqr;
        t = Mathf.Clamp01(t);

        return line.Start + ab * t;
    }

    public override void OnShootPress(bool isInstant,bool hasBall,bool ballCanAirInteract) {
        if (hasBall)
        {
            playerSim.SwitchState(PlayerState.PREPPING_SHOT);
        }
        else if (ballCanAirInteract)
        {
            if (playerSim.IsFacingTargetGoal())
            {
                playerSim.SwitchState(PlayerState.VOLLEY_KICK);
                playerSim.SwitchState(PlayerState.HEADER);
            }
            else
            {
                playerSim.SwitchState(PlayerState.BICYCLE_KICK);
            }
        }
    }

    public override void OnShootRelease() {
        PlayerStateData data = PlayerStateData.Build()
            .SetShotPower(playerSim.Power)
            .SetShotDirection(_moveDirection)
            .SetIsInstant(true);
        playerSim.SwitchState(PlayerState.SHOOTING, data);
    }

    public override bool CanCarryBall() {
        return playerSim.role != Role.GOALIE;
    }
}
