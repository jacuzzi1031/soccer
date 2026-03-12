
    using System.Collections.Generic;
    using Unity.VisualScripting;
    using UnityEngine;

    public class CollisionSystem:ISimulationSystem {
        public float ballRadius;
        public float playerRadius;
        public float ballCaptureRadius;
        public List<LineSegment> lines;
        public List<PlayerSim> teamHome;
        public List<PlayerSim> teamAway;
        public List<PlayerSim> team;
        public BallSim ball;
        private const float MAX_CAPTURE_HEIGHT = 8f;
        private const float BALL_CONTROL_HEIGHT_MAX = 4.3f;
        public Vector2 playerForBallOffset = new Vector2(0, 4f);
        public void Tick(SimulationContext context) {
            DetectBallForPlayer(context);
        }
        private void DetectBallForPlayer(SimulationContext context) {
            if (!ball.currentState.CanCarriedBall())
                return;
            
            foreach (var player in team)
            {
                if (!player.CanCarryBall())
                    continue;
            
                float distSqr = (player.Position+playerForBallOffset - ball.Position).sqrMagnitude;
            
                float captureRadius = playerRadius + ballCaptureRadius;
            
                if (distSqr < captureRadius * captureRadius && ball.height < MAX_CAPTURE_HEIGHT) {
                    player.currentState.VolleyShot();
                    if (player.CanCarryBall()) {
                        
                        if (ball.height > BALL_CONTROL_HEIGHT_MAX) {
                            player.SwitchState(PlayerState.CHEST_CONTROL);
                        }
                        ball.carrier = player;
                        ball.SwitchState(BallState.CARRIED);
                        context._simulationModel.PlayerSystem.OnPlayerBecomesCarrier(player.playerId,player.isHome);
                        break;
                    }
                }
            }
        }
        
        public void RegisterTeams(List<PlayerSim> home, List<PlayerSim> away, SimulationConfig simConfig,BallSim ballSim, List<LineSegment> lineSegments) {
            teamHome=home;
            teamAway=away;
            team=new List<PlayerSim>();
            team.AddRange(home);
            team.AddRange(away);
            ballRadius=simConfig.BallRadius;
            playerRadius=simConfig.PlayerRadius;
            ballCaptureRadius=simConfig.ballCaptureRadius;
            lines = lineSegments;
            ball=ballSim;
        }
        public void Stop() {
        }
    }
