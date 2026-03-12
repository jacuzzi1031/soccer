
    using UnityEngine;

    public class PlayerStateHeader:PlayerSimState {
        private float _elapsedTicks;
        private float _durationTicks=0.2f;

        public override void OnEnter() {
            _elapsedTicks = 0f;
        }

        public override void _Update(float deltaTime) {
            _elapsedTicks+=deltaTime;

            if (_elapsedTicks >= _durationTicks)
            {
                playerSim.SwitchState(PlayerState.RECOVERING);
            }
        }

        public override void VolleyShot() {
            Vector2 destination = playerSim.GetFarTargetPosition();
            Vector2 direction = (destination - playerSim.Position).normalized;
            _ballSim.shoot( playerSim.Power * BONUS_POWER*direction);
        }
    }
