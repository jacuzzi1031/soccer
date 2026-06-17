using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

namespace Games.Player.AIBehavior{
    public class AIBehavior
    {  
        protected FixedVector2 moveDir=FixedVector2.Zero;
        protected BallSim ballSim;
        protected TriggerDetection opponentDetectionArea;
        protected PlayerSim playerSim;
        private FixedFloat nextAiTickTime;
        private int aiFrameCounter = 0;
        private const int aiTickIntervalFrames = 12; 
        protected FixedRect GoalArea;
        public int homeCount;
        public int awayCount;
        public int matchPlayerCount;
        public Role role;
        
        protected static FixedFloat PASS_PROBABILITY = (FixedFloat)0.05f;
        protected static FixedFloat  SHOT_DISTANCE = (FixedFloat)150f;
        protected static FixedFloat SHOT_PROBABILITY = (FixedFloat)0.3f;
        protected static FixedFloat SPREAD_ASSIST_FACTOR = (FixedFloat)2f;
        protected static FixedFloat TACKLE_DISTANCE = (FixedFloat)15f;
        protected static FixedFloat TACKLE_PROBABILITY = (FixedFloat)0.3f;

        protected static FixedFloat GoalYOffset     = (FixedFloat)10f;
        protected static FixedFloat GoalYRangeSlack = (FixedFloat)10f;
        protected static FixedFloat ReboundOffsetX  = (FixedFloat)100f;
        protected FixedFloat SteeringForceFactor=FixedFloat.One;

        public void UpdateAI()
        {
            aiFrameCounter++;

            if (aiFrameCounter >= aiTickIntervalFrames)
            {
                aiFrameCounter -= aiTickIntervalFrames;

                PerformAIMovement();
                PerformAIDecisions();
            }
        }
        public virtual void PerformAIMovement() {
        }
        public virtual void PerformAIDecisions() {
        }
        public FixedVector2 GetAIMoveDir() {
            return moveDir;
        }

        public void Setup(PlayerSim playerSim, BallSim ballSim, FixedRect GoalArea, int MatchPlayerCount)
        {
            this.playerSim = playerSim;
            this.ballSim = ballSim;
            this.GoalArea = GoalArea;
            matchPlayerCount = MatchPlayerCount;
            role=playerSim.role;

            float r = HashRandom(playerSim.playerId);

            // 随机初始偏移（0 ~ interval-1 帧）
            aiFrameCounter = (int)(r * aiTickIntervalFrames);
        }
        public float HashRandom(int seed)
        {
            uint x = (uint)seed;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;

            return (x & 0xFFFFFF) / (float)0x1000000;
        }
        protected FixedVector2 GetCarrierSteeringForce()
        {
            FixedVector2 target = playerSim.GetCenterTargetPosition();
            var playerPos = playerSim.Position;
            FixedVector2 direction = (target - playerPos).normalized;
            FixedFloat weight = GetBiCircularWeight(playerPos, target, 100, 0, 150, 1);
            return direction * weight;
        }
        protected FixedFloat GetBiCircularWeight(FixedVector2 pos, FixedVector2 target, FixedFloat r1, FixedFloat w1, FixedFloat r2, FixedFloat w2)
        {
            FixedFloat d = FixedVector2.Distance(pos, target);
    
            if (d < r1) return w1;
            if (d > r2) return w2;
    
            FixedFloat t = (d - r1) / (r2 - r1);
            return FixedMath.Lerp(w1, w2, t);
        }
        protected bool IsBallCarriedByTeammate()
        {
            return ballSim.carrier != null && ballSim.carrier.isHome == playerSim.isHome;
        }
        protected bool TeammateIntoShootingScope() {
            FixedVector2 target = playerSim.GetCenterTargetPosition();
            return FixedVector2.Distance(ballSim.carrier.Position, target) < SHOT_DISTANCE;
        }
        protected FixedVector2 GetDefenseTarget(FixedFloat offsetX)
        {
            FixedVector2 centerPos = playerSim.GetCenterTargetPosition();
            
            FixedVector2 target = centerPos;

            FixedFloat spawnY = playerSim.spawnPosition.y;
            target.y = spawnY > 0 ? GoalYRangeSlack + spawnY : -GoalYRangeSlack + spawnY;

            // 回撤偏移
            target.x = centerPos.x - FixedFloat.Sign(centerPos.x) * offsetX;

            return target;
        }
        protected FixedVector2 GetAttackTarget(FixedFloat offsetX)
        {
            FixedVector2 topPos    = playerSim.GetTopTargetPosition();
            FixedVector2 bottomPos = playerSim.GetBottomTargetPosition();
            FixedVector2 centerPos = playerSim.GetCenterTargetPosition();

            FixedVector2 carrierPos = ballSim.carrier.Position;

            FixedVector2 target = centerPos;

            bool outOfGoalYRange =
                carrierPos.y > topPos.y + GoalYRangeSlack ||
                carrierPos.y < bottomPos.y - GoalYRangeSlack;

            if (!outOfGoalYRange)
            {
                FixedFloat distToTop    = FixedFloat.Abs(carrierPos.y - topPos.y);
                FixedFloat distToBottom = FixedFloat.Abs(carrierPos.y - bottomPos.y);

                target.y = distToTop > distToBottom
                    ? topPos.y + GoalYOffset
                    : bottomPos.y - GoalYOffset;
            }

            // 回撤偏移
            target.x = centerPos.x - FixedFloat.Sign(centerPos.x) * offsetX;

            return target;
        }
        protected FixedVector2 GetOnDutySteeringForce(Role role) {
            switch (role) {
                case Role.ATTACKER:
                    SteeringForceFactor = (FixedFloat)0.6f;
                    break;
                case Role.MIDFIELD:
                    SteeringForceFactor = (FixedFloat)0.8f;
                    break;
                case Role.DEFENSE:
                    SteeringForceFactor = FixedFloat.One;
                    break;
            }
            return SteeringForceFactor*playerSim.weightOnDutySteering *
                   (ballSim.Position - playerSim.Position).normalized;
        }
        protected bool IsBallPossessedByOpponent()
        {
            return ballSim.carrier != null && ballSim.carrier.isHome != playerSim.isHome;
        }
        protected FixedVector2 GetSpawnSteeringForce(Role role) {
            FixedFloat factor=FixedFloat.One;
            switch (role) {
                case Role.ATTACKER:
                    factor = (FixedFloat)1.5f;
                    break;
                case Role.MIDFIELD:
                    break;
                case Role.DEFENSE:
                    factor = (FixedFloat)0.8f;
                    break;
            }
            var playerPos = playerSim.Position;
            var spawnPos = playerSim.spawnPosition;
            FixedFloat weight = GetBiCircularWeight(playerPos, spawnPos, 20, (FixedFloat)0.2f, 50, 1);
            FixedVector2 direction = (spawnPos - playerPos).normalized;
            return factor*direction * weight;
        }
        protected FixedVector2 GetAttackHoldForce(Role role) {
            
            FixedVector2 target=FixedVector2.Zero;
            switch (role) {
                case Role.ATTACKER:
                    target=GetDefenseTarget(ReboundOffsetX*2);
                    break;
                case Role.MIDFIELD:
                    target=GetDefenseTarget(ReboundOffsetX*3);
                    break;
            }
            FixedVector2 dir =
                (target - playerSim.Position).normalized;

            return dir;
        }
        protected FixedVector2 GetBallProximitySteeringForce()
        {
            var playerPos = playerSim.Position;
            var carrierPos = ballSim.Position;
            FixedFloat weight = GetBiCircularWeight(playerPos, carrierPos, 20, 1, 120, 0);
            FixedVector2 direction = (carrierPos - playerPos).normalized;
    
            return direction * weight;
        }
        //一个队不一拥而上
        protected FixedVector2 GetDensityAroundBallSteeringForce()
        {
            int count = playerSim.isHome?homeCount:awayCount;
            if (count == 0) return FixedVector2.Zero;
            
            FixedFloat weight =(FixedFloat)( 1 - (1f / count));
            FixedVector2 direction = (playerSim.Position - ballSim.Position).normalized;
            
            return (FixedFloat)1.5f*weight*direction;
            
        }
        protected bool HasOpponentsNearby()
        {
            int count=playerSim.isHome?awayCount:homeCount;
            return count>0;
        }

        protected FixedFloat getPassProbability(Role role) {
            if (role != Role.DEFENSE) {
                return PASS_PROBABILITY * (FixedFloat)0.5f;
            }

            return PASS_PROBABILITY;
        }
    }
}


