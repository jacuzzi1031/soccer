using System.Collections.Generic;
using Net.Core.Simulation.SimSignal;
using Net.FixFloat;
using UnityEngine;

public class BoundarySystem : ISimulationSystem{
    public FixedFloat ballRadius;
    public FixedFloat playerRadius;
    public FixedFloat playerVerticalOffset;
    public List<LineSegment> playerLines;
    public List<LineSegment> ballLines;
    public List<LineSegment> scoreLines;
    public List<LineSegment> stopballLines;

    public List<PlayerSim> team;
    public List<PlayerSim> goalies = new List<PlayerSim>();
    public BallSim ball;
    public CommandBuffer commandBuffer;
    public SimEventBus _eventBus;
    public static readonly FixedFloat BOUNCINESS =
        (FixedFloat)0.8f;

    bool goalTriggered = false;

    public FixedVector2 playerInnerOffset =
        new FixedVector2(0, (FixedFloat)8f);

    public BoundarySystem(SimEventBus eventBus) {
        _eventBus = eventBus;
    }

    public void Tick(SimulationContext context) {
        foreach (var player in team) {
            FixedVector2 center = player.Position + playerInnerOffset;

            center = ResolveBoundary(center);

            player.Position = center - playerInnerOffset;
        }

        ResolveBallBoundary();

        var ballPos = ball.Position;
        //for score
        foreach (var line in scoreLines) {
            FixedVector2 closest = ClosestPointOnSegment(ballPos, line);

            FixedVector2 diff = ballPos - closest;

            if (!goalTriggered && diff.sqrMagnitude < ballRadius * ballRadius) {
                goalTriggered = true;
                bool isHome = line.Start.x > 0;
                commandBuffer.Enqueue(new SimulationCommand
                    { Type = SimulationCommandType.TeamScoring, isHome = isHome });
                _eventBus.Publish(new TeamScoringSignal());
                break;
            }
            else if (goalTriggered && context.ResetBall) {
                goalTriggered = false;
            }
        }

        //for stopball
        foreach (var line in stopballLines) {
            FixedVector2 closest = ClosestPointOnSegment(ballPos, line);

            FixedVector2 diff = ballPos - closest;

            if (diff.sqrMagnitude < ballRadius * ballRadius) {
                ball.Velocity = FixedVector2.Zero;
                break;
            }
        }

        //for goalkeeper
        if (ball.ballState == BallState.CARRIED) {
            return;
        }

        foreach (var goalKeeper in goalies) {
            ResolveBallGoalieCollision(goalKeeper.playerId,goalKeeper.Position, ball.Position, ball.Velocity);
        }
    }

    void ResolveBallGoalieCollision(int goalKeeperId,FixedVector2 center, FixedVector2 ballPos, FixedVector2 ballVelocity) {
        FixedFloat halfHeight = playerVerticalOffset;
        FixedFloat radius = playerRadius;

        FixedVector2 top = center + FixedVector2.Up * halfHeight;

        FixedVector2 bottom = center;

        FixedVector2 closest = ClosestPointOnPlayerSegment(bottom, top, ballPos);

        FixedFloat dist = FixedVector2.Distance(ballPos, closest);
        FixedFloat combinedRadius = ballRadius + radius;

        if (dist < combinedRadius) {
            FixedVector2 normal = (ballPos - closest).normalized;

            ballVelocity = FixedVector2.Reflect(ballVelocity, normal);
            _eventBus.Publish( new PlayStyleShowSignal( goalKeeperId, PlayerState.DIVING) );
            _eventBus.Publish( new goalKeeperBounceBallSignal() );
            // 推出重叠
            FixedFloat penetration = combinedRadius - dist;
            ballPos += normal * penetration;
            ball.Velocity = ballVelocity * (FixedFloat)0.9f;
            ball.Position = ballPos;
        }
    }

    FixedVector2 ClosestPointOnPlayerSegment(
        FixedVector2 a,
        FixedVector2 b,
        FixedVector2 p) {
        FixedVector2 ab = b - a;

        FixedFloat t =
            FixedVector2.Dot(p - a, ab)
            / ab.sqrMagnitude;

        t = FixedFloat.Clamp(
            t,
            FixedFloat.Zero,
            FixedFloat.One);

        return a + ab * t;
    }

    private void ResolveBallBoundary() {
        FixedVector2 position = ball.Position;

        FixedFloat bestPenetration =
            FixedFloat.Zero;

        FixedVector2 bestNormal =
            FixedVector2.Zero;

        FixedVector2 bestPoint =
            FixedVector2.Zero;

        foreach (var line in ballLines) {
            FixedVector2 closest = ClosestPointOnSegment(position, line);

            FixedVector2 diff = position - closest;
            FixedFloat sqrDist = diff.sqrMagnitude;

            FixedFloat r2 =
                ballRadius * ballRadius;

            if (sqrDist < r2) {
                FixedFloat dist = diff.magnitude;

                FixedVector2 normal = dist > (FixedFloat)0.00001f
                    ? diff / dist
                    : (line.End - line.Start).normalized;

                FixedFloat penetration =
                    ballRadius - dist;

                if (penetration > bestPenetration) {
                    bestPenetration = penetration;
                    bestNormal = normal;
                    bestPoint = closest;
                }
            }
        }

        if (bestPenetration > (FixedFloat)0f) {
            ball.Position = bestPoint + bestNormal * ballRadius;

            ball.Velocity =
                FixedVector2.Reflect(
                    ball.Velocity,
                    bestNormal)
                * BOUNCINESS;
        }
    }


    private FixedVector2 ResolveBoundary(FixedVector2 playerPosition) {
        foreach (var line in playerLines) {
            FixedVector2 closest = ClosestPointOnSegment(playerPosition, line);

            FixedVector2 diff = playerPosition - closest;
            FixedFloat sqrDist = diff.sqrMagnitude;

            if (sqrDist < playerRadius * playerRadius) {
                if (sqrDist < FixedFloat.Zero)
                    continue;

                FixedFloat dist = diff.magnitude;
                FixedVector2 normal = diff / dist;


                playerPosition = closest + normal * playerRadius;
            }
        }

        return playerPosition;
    }

    FixedVector2 ClosestPointOnSegment(FixedVector2 p, LineSegment line) {
        FixedVector2 ap = p - line.Start;

        FixedFloat t =
            FixedVector2.Dot(ap, line.Edge)
            / line.EdgeSqr;
        t = FixedFloat.Clamp(
            t,
            FixedFloat.Zero,
            FixedFloat.One);

        return line.Start + line.Edge * t;
    }

    public void RegisterTeams(List<PlayerSim> home, List<PlayerSim> away, CommandBuffer CommandBuffer,
        SimulationConfig simConfig, BallSim ballSim,
        List<LineSegment> PlayerLines, List<LineSegment> BallLines, List<LineSegment> ScoreLines,
        List<LineSegment> StopballLines) {
        team = new List<PlayerSim>();
        team.AddRange(home);
        team.AddRange(away);
        ballRadius = simConfig.BallRadius;
        playerRadius = simConfig.PlayerRadius;
        playerVerticalOffset = simConfig.playerVerticalOffset;
        ball = ballSim;
        playerLines = PlayerLines;
        ballLines = BallLines;
        scoreLines = ScoreLines;
        stopballLines = StopballLines;
        commandBuffer = CommandBuffer;
        foreach (PlayerSim player in team) {
            if (player.role == Role.GOALIE) {
                goalies.Add(player);
            }
        }
    }

    public void Stop() {
    }
}