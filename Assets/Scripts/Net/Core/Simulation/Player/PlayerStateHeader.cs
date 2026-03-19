
    using UnityEngine;

    public class PlayerStateHeader:PlayerSimState {
        private float _elapsedTicks;
        // private float _durationTicks=0.2f;
        private float _durationTicks=0.4f;
        public const float HEIGHT_START = 0.1f;
        public const float HEIGHT_VELOCITY_START = 10f;
        public override void OnEnter() {
            _elapsedTicks = 0f;
            playerSim.Height = HEIGHT_START;
            playerSim.HeightVelocity = HEIGHT_VELOCITY_START;
        }

        public override void _Update(float deltaTime) {
            _elapsedTicks+=deltaTime;

            if (_elapsedTicks >= _durationTicks)
            {
                playerSim.SwitchState(PlayerState.RECOVERING);
            }
        }

        public override bool VolleyShot() {
            Vector2 destination = playerSim.GetFarTargetPosition();
            Vector2 direction = (destination - playerSim.Position).normalized;
            _ballSim.shoot( playerSim.Power * BONUS_POWER*direction);
            return true;
        }
        public override bool CanCarryBall() {
            return true;
        }
    }
