using Net.FixFloat;

namespace Games.Player.AIBehavior{
    public class AIBehaviorAttacker: AIBehavior{
        public override void PerformAIMovement()
        {
            moveDir = FixedVector2.Zero;
    
            if (playerSim.HasBall())
            {
                moveDir += GetCarrierSteeringForce();
            } 
            //队友持球
            else if (IsBallCarriedByTeammate())
            {
                //队友禁区跑位
                if (TeammateIntoShootingScope()) {
                        moveDir += GetCarrierReboundShotForce();
                }
                else {
                    //队友推进
                    moveDir += GetCarrierAssistForce();
                }
            }
            //对手持球或者无人持球
            else
            {
                //依次上抢
                moveDir += GetOnDutySteeringForce(role);
                //一个队不一拥而上
                moveDir += GetDensityAroundBallSteeringForce();
                if (IsBallPossessedByOpponent()) {
                    //留在前场
                    moveDir += GetAttackHoldForce(role);
                }
                else if (ballSim.carrier == null)
                {
                    ////无人持球范围内全力上抢
                    moveDir += GetBallProximitySteeringForce();

                }
            }
            moveDir = FixedVector2.ClampMagnitude(moveDir, FixedFloat.One);
        }
        FixedVector2 GetCarrierAssistForce()
        {
            FixedVector2 target = GetAttackTarget(ReboundOffsetX*(FixedFloat)1.5f);

            FixedVector2 direction =
                (target - playerSim.Position).normalized;

            FixedFloat weight = GetBiCircularWeight(
                playerSim.Position,
                target,
                1,
                0,
                20,
                1);

            return direction * weight;
        }
        FixedVector2 GetCarrierReboundShotForce()
        {
            FixedVector2 target = GetAttackTarget(ReboundOffsetX);

            FixedVector2 direction =
                (target - playerSim.Position).normalized;

            FixedFloat weight = GetBiCircularWeight(
                playerSim.Position,
                target,
                1,
                0,
                20,
                1);

            return direction * weight;
        }

        public override void PerformAIDecisions() {
            var ballPos = ballSim.Position;
            var playerPos = playerSim.Position;
            if (IsBallPossessedByOpponent() &&
                FixedVector2.Distance(playerPos, ballPos) < TACKLE_DISTANCE &&
                FixedMath.Roll(TACKLE_PROBABILITY))
            {
                playerSim.SwitchState(PlayerState.TACKLING);
            }
            //玩家控制的队伍不会射门传球
            if (playerSim.isHome || !playerSim.isHome && matchPlayerCount > 1) {
                return;
            }
        
            if (ballSim.carrier?.playerId == playerSim.playerId)
            {
                FixedVector2 target = playerSim.GetCenterTargetPosition();
            
            
                if (FixedVector2.Distance(playerPos, target) < SHOT_DISTANCE &&
                    FixedMath.Roll(SHOT_PROBABILITY) )
                {
                    FixedVector2 direction = (playerSim.GetFarTargetPosition() - playerPos).normalized;
    
                    var data = new PlayerStateData()
                        .SetShotPower(playerSim.Power)
                        .SetShotDirection(direction);
    
                    playerSim.SwitchState(PlayerState.SHOOTING, data);
                }
                else if ( FixedMath.Roll(getPassProbability(role)) && HasOpponentsNearby())
                {
                    //汤球
                    playerSim.SwitchState(PlayerState.PASSING);
                
                    //或者回传
                }
            }
        }
    }
}