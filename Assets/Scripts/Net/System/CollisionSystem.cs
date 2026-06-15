using System.Collections.Generic;
using Net.FixFloat;
using Unity.VisualScripting;
using UnityEngine;

public class CollisionSystem : ISimulationSystem{
    public FixedFloat ballRadius;
    public FixedFloat playerRadius;
    public FixedFloat ballCaptureRadius;
    public List<LineSegment> lines;
    public List<PlayerSim> teamHome;
    public List<PlayerSim> teamAway;
    public List<PlayerSim> team;
    public BallSim ball;
    public FixedFloat captureRadiusSqr;
    public FixedFloat volleycaptureRadiusSqr;

    //实际  ballView height*2.5f
    private static readonly FixedFloat MAX_CAPTURE_HEIGHT = (FixedFloat)0.6f;
    private static readonly FixedFloat BALL_CONTROL_HEIGHT_MAX = (FixedFloat)0.5f;

    public FixedVector2 playerForBallOffset = new FixedVector2( FixedFloat.Zero, (FixedFloat)4f);

    public FixedVector2 attackRightOffset = new FixedVector2( (FixedFloat)16f, (FixedFloat)16f );

    public FixedVector2 attackLeftOffset = new FixedVector2( (FixedFloat)(-16f), (FixedFloat)16f );

    public FixedVector2 targetRightOffset = new FixedVector2( (FixedFloat)10.8f, (FixedFloat)4f );

    public FixedVector2 targetLeftOffset = new FixedVector2( (FixedFloat)(-10.8f), (FixedFloat)4f );

    public FixedVector2 acceptOffset = new FixedVector2( FixedFloat.Zero, (FixedFloat)6f );

    public FixedFloat tacklingRadiusSqr;
    public FixedFloat ballRadiusSqr;

    public void Tick(SimulationContext context) {
        DetectBallForPlayer(context);

        DetectTackle(teamHome, teamAway);
        DetectTackle(teamAway, teamHome);
    }

    private void DetectTackle(List<PlayerSim> teamAttack, List<PlayerSim> teamtarget) {
        foreach (var attacker in teamAttack) {
            if (!attacker.currentState.IsDamageEmitter())
                continue;
            foreach (var target in teamtarget) {
                if (!DistanceQualify(attacker, target))
                    continue;
                if (target.currentState.CouldHurt()) {
                    target.SwitchState(PlayerState.HURT,
                        PlayerStateData.Build().SetMoveDir(attacker.Velocity.normalized));
                }
            }
        }
    }

    private bool DistanceQualify(PlayerSim attacker, PlayerSim target) {
        var attackOffset = attacker.HeadingRight ? attackRightOffset : attackLeftOffset;
        FixedVector2 attckerPos = attacker.Position + attackOffset;
        FixedFloat distSqr = (attacker.Position - target.Position).sqrMagnitude;
        if (distSqr < tacklingRadiusSqr) {
            return true;
        }

        var targetOffset = target.HeadingRight ? targetRightOffset : targetLeftOffset;
        FixedVector2 targetPos2 = target.Position + targetOffset;
        FixedFloat distSqr2 = (attckerPos - targetPos2).sqrMagnitude;
        if (distSqr2 < ballRadiusSqr) {
            return true;
        }

        return false;
    }

    private void DetectBallForPlayer(SimulationContext context) {
        if (!ball.currentState.CanCarriedBall())
            return;

        foreach (var player in team) {
            if (!player.CanCarryBall()) {
                continue;
            }

            FixedFloat distSqr = (player.Position + playerForBallOffset - ball.Position).sqrMagnitude;
            if (distSqr < volleycaptureRadiusSqr && ball.height < MAX_CAPTURE_HEIGHT) {
                bool hasVolleyShot = player.currentState.VolleyShot();
                if (hasVolleyShot) break;
            }

            if (distSqr < captureRadiusSqr && ball.height < MAX_CAPTURE_HEIGHT) {
                if (ball.height > BALL_CONTROL_HEIGHT_MAX) {
                    player.SwitchState(PlayerState.CHEST_CONTROL);
                }

                ball.carrier = player;
                ball.SwitchState(BallState.CARRIED);

                context._simulationModel.PlayerSystem.OnPlayerBecomesCarrier(player.playerId, player.isHome,
                    ball.firstPlayerCarryBall);
                if (!ball.firstPlayerCarryBall) {
                    ball.firstPlayerCarryBall = true;
                }

                break;
            }
        }
    }

    public void RegisterTeams(List<PlayerSim> home, List<PlayerSim> away, SimulationConfig simConfig, BallSim ballSim,
        List<LineSegment> lineSegments) {
        teamHome = home;
        teamAway = away;
        team = new List<PlayerSim>();
        team.AddRange(home);
        team.AddRange(away);
        ballRadius = simConfig.BallRadius;
        playerRadius = simConfig.PlayerRadius;
        ballCaptureRadius = simConfig.ballCaptureRadius;
        captureRadiusSqr = (playerRadius + ballCaptureRadius) * (playerRadius + ballCaptureRadius);
        volleycaptureRadiusSqr = (simConfig.playervolleyRadius + ballCaptureRadius) *
                                 (simConfig.playervolleyRadius + ballCaptureRadius);
        tacklingRadiusSqr = (playerRadius + (FixedFloat)4f) * (playerRadius +(FixedFloat) 4f);
        ballRadiusSqr = (ballRadius + (FixedFloat)4f) * (ballRadius + (FixedFloat)4f);
        lines = lineSegments;
        ball = ballSim;
    }

    public void Stop() {
    }
}