
    using Net.FixFloat;
    using UnityEngine;

    public class PlayerStateHeader:PlayerSimState {
        private const int DURATION_FRAMES = 16;

        private int _elapsedFrames;

        public static readonly FixedFloat HEIGHT_START = (FixedFloat)0.1f;
        public static readonly FixedFloat HEIGHT_VELOCITY_START = (FixedFloat)10f;

        public override void OnEnter()
        {
            _elapsedFrames = 0;

            playerSim.Height = HEIGHT_START;
            playerSim.HeightVelocity = HEIGHT_VELOCITY_START;
        }

        public override void _Update()
        {
            _elapsedFrames++;

            if (_elapsedFrames >= DURATION_FRAMES)
            {
                playerSim.SwitchState(PlayerState.RECOVERING);
            }
        }

        public override bool VolleyShot()
        {
            FixedVector2 destination = playerSim.GetFarTargetPosition();

            FixedVector2 direction =
                (destination - playerSim.Position).normalized;

            _ballSim.shoot(
                direction * playerSim.Power * BONUS_POWER);

            return true;
        }
        public override bool CanCarryBall() {
            return true;
        }
    }
