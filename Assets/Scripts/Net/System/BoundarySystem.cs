

    using System.Collections.Generic;
    using UnityEngine;

    public class BoundarySystem:ISimulationSystem {
        public float ballRadius;
        public float playerRadius;
        public List<LineSegment> playerLines;
        public List<LineSegment> ballLines;
        public List<LineSegment> scoreLines;
        public List<LineSegment> stopballLines;
        
        public List<PlayerSim> team;
        public BallSim ball;
        public CommandBuffer commandBuffer;
        public const float BOUNCINESS = 0.8f;
        bool goalTriggered=false;
        public Vector2 playerInnerOffset=new Vector2(0,8f);

        public void Tick(SimulationContext context) {
            foreach (var player in team) {
                Vector2 center = player.Position + playerInnerOffset;

                center = ResolveBoundary(center);

                player.Position = center - playerInnerOffset;
            }
            ResolveBallBoundary();
            
            var ballPos=ball.Position;
            //for score
            foreach (var line in scoreLines)
            {
                Vector2 closest = ClosestPointOnSegment(ballPos, line);

                Vector2 diff = ballPos - closest;

                if (!goalTriggered && diff.sqrMagnitude < ballRadius*ballRadius)
                {
                    goalTriggered = true;
                    bool isHome = line.Start.x > 0;
                    commandBuffer.Enqueue(new SimulationCommand{Type=SimulationCommandType.TeamScoring,isHome=isHome});
                    break;
                }
                else if(goalTriggered && context.ResetBall){
                    goalTriggered = false;
                }
            }
            
            //for stopball
            foreach (var line in stopballLines)
            {
                Vector2 closest = ClosestPointOnSegment(ballPos, line);

                Vector2 diff = ballPos - closest;

                if (diff.sqrMagnitude < ballRadius*ballRadius)
                {
                    ball.Velocity = Vector2.zero;
                    break;
                }
            }
        }

        private void ResolveBallBoundary()
        {
            Vector2 position = ball.Position;

            float bestPenetration = 0f;
            Vector2 bestNormal = Vector2.zero;
            Vector2 bestPoint = Vector2.zero;

            foreach (var line in ballLines)
            {
                Vector2 closest = ClosestPointOnSegment(position, line);

                Vector2 diff = position - closest;
                float sqrDist = diff.sqrMagnitude;

                float r2 = ballRadius * ballRadius;

                if (sqrDist < r2)
                {
                    float dist = Mathf.Sqrt(sqrDist);

                    Vector2 normal = dist > 0.00001f
                        ? diff / dist
                        : (line.End - line.Start).normalized;

                    float penetration = ballRadius - dist;

                    if (penetration > bestPenetration)
                    {
                        bestPenetration = penetration;
                        bestNormal = normal;
                        bestPoint = closest;
                    }
                }
            }

            if (bestPenetration > 0f)
            {
                ball.Position = bestPoint + bestNormal * ballRadius;

                ball.Velocity = Vector2.Reflect(ball.Velocity, bestNormal) * BOUNCINESS;
            }
        }


        private Vector2 ResolveBoundary(Vector2 playerPosition) {
            foreach (var line in playerLines)
            {
                Vector2 closest = ClosestPointOnSegment(playerPosition, line);

                Vector2 diff = playerPosition - closest;
                float sqrDist = diff.sqrMagnitude;

                if (sqrDist < playerRadius * playerRadius)
                {
                    if (sqrDist < 0.0000001f)
                        continue;

                    float dist = Mathf.Sqrt(sqrDist);
                    Vector2 normal = diff / dist;
                
                    playerPosition = closest + normal * playerRadius;
                }
            }

            return playerPosition;
        }
        Vector2 ClosestPointOnSegment(Vector2 p, LineSegment line)
        {
            Vector2 ap = p - line.Start;

            float t = Vector2.Dot(ap, line.Edge) / line.EdgeSqr;
            t = Mathf.Clamp01(t);

            return line.Start + line.Edge * t;
        }

        public void RegisterTeams(List<PlayerSim> home, List<PlayerSim> away,CommandBuffer CommandBuffer, SimulationConfig simConfig,BallSim ballSim,
            List<LineSegment> PlayerLines,List<LineSegment>  BallLines,List<LineSegment>  ScoreLines,List<LineSegment>  StopballLines)
        {
            team=new List<PlayerSim>();
            team.AddRange(home);
            team.AddRange(away);
            ballRadius=simConfig.BallRadius;
            playerRadius=simConfig.PlayerRadius;
            ball = ballSim;
            playerLines = PlayerLines;
            ballLines = BallLines;
            scoreLines=ScoreLines;
            stopballLines=StopballLines;
            commandBuffer=CommandBuffer;
        }
        public void Stop() {
        }
    }
