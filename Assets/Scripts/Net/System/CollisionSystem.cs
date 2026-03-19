
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
        public float captureRadiusSqr;
        public float volleycaptureRadiusSqr;

        //实际  ballView height*2.5f
        private const float MAX_CAPTURE_HEIGHT = 8f;  //20
        private const float BALL_CONTROL_HEIGHT_MAX = 4.3f;
        public Vector2 playerForBallOffset = new Vector2(0, 4f);
        public Vector2 attackRightOffset=new Vector2(16f,16f);
        public Vector2 attackLeftOffset=new Vector2(-16f,16f);
        public Vector2 targetRightOffset=new Vector2(10.8f, 4f);
        public Vector2 targetLeftOffset=new Vector2(-10.8f, 4f);
        public Vector2 acceptOffset=new Vector2(0f,12f);
        public float tacklingRadiusSqr;
        public float ballRadiusSqr;
        
        public void Tick(SimulationContext context) {
            DetectBallForPlayer(context);

            DetectTackle(teamHome,teamAway);
            DetectTackle(teamAway,teamHome);
        }

        private void DetectTackle(List<PlayerSim> teamAttack,List<PlayerSim> teamtarget) {
            
            foreach (var attacker in teamAttack) {
                
                if (!attacker.currentState.IsDamageEmitter())
                    continue;
                foreach (var target in teamtarget) {
                    if(!DistanceQualify(attacker,target))
                        continue;
                    if (target.currentState.CouldHurt()) {
                        target.SwitchState(PlayerState.HURT, 
                            PlayerStateData.Build().SetMoveDir(attacker.Velocity.normalized));
                    }
                }
            }
        }

        private bool DistanceQualify(PlayerSim attacker, PlayerSim target) {
            var attackOffset= attacker.HeadingRight?attackRightOffset: attackLeftOffset;
            Vector2 attckerPos = attacker.Position + attackOffset;
            Vector2 targetPos1=target.Position+acceptOffset;
            float distSqr= (attckerPos-targetPos1).sqrMagnitude;
            if (distSqr < tacklingRadiusSqr) {
                return true;
            } 
            var targetOffset= target.HeadingRight?targetRightOffset: targetLeftOffset;
            Vector2 targetPos2=target.Position+targetOffset;
            float distSqr2= (attckerPos-targetPos2).sqrMagnitude;
            if (distSqr2 < ballRadiusSqr) {
                return true;
            } 
            return false;
        }

        private void DetectBallForPlayer(SimulationContext context) {
            if (!ball.currentState.CanCarriedBall())
                return;
            
            foreach (var player in team)
            {
                if (!player.CanCarryBall()) {
                    continue;
                }
                float distSqr = (player.Position+playerForBallOffset - ball.Position).sqrMagnitude;
                if (distSqr < volleycaptureRadiusSqr && ball.height < MAX_CAPTURE_HEIGHT) {
                    bool hasVolleyShot = player.currentState.VolleyShot();
                    if(hasVolleyShot) continue;
                }

                if (distSqr < captureRadiusSqr && ball.height < MAX_CAPTURE_HEIGHT) {
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
        
        public void RegisterTeams(List<PlayerSim> home, List<PlayerSim> away, SimulationConfig simConfig,BallSim ballSim, List<LineSegment> lineSegments) {
            teamHome=home;
            teamAway=away;
            team=new List<PlayerSim>();
            team.AddRange(home);
            team.AddRange(away);
            ballRadius=simConfig.BallRadius;
            playerRadius=simConfig.PlayerRadius;
            ballCaptureRadius=simConfig.ballCaptureRadius;
            captureRadiusSqr = (playerRadius + ballCaptureRadius) * (playerRadius + ballCaptureRadius);
            volleycaptureRadiusSqr = (simConfig.playervolleyRadius + ballCaptureRadius) * (simConfig.playervolleyRadius + ballCaptureRadius);
            tacklingRadiusSqr=playerRadius*playerRadius;
            ballRadiusSqr = ballRadius * ballRadius;
            lines = lineSegments;
            ball=ballSim;
        }
        public void Stop() {
        }
    }
