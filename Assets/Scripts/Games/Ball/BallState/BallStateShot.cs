
    using UnityEngine;

    public class BallStateShot: BallState {
        private const float SHOT_SPRITE_SCALE = 0.8f;
        private const float SHOT_HEIGHT = 5f;
        private const float DURATION_SHOT = 1.0f;
        private float timeSinceShot;
        public override void OnEnter() {
            SetBallAnimationFromVelocity();
            ballSprite.localScale = new Vector3(1, SHOT_SPRITE_SCALE, 1);
            ball.height = SHOT_HEIGHT;
            timeSinceShot=Time.time;

            shotParticles.Play();
        }

        public override void _Update() {
            if (Time.time - timeSinceShot >= DURATION_SHOT) {
                TransitionState(Ball.State.FREEFORM);
            }
        }

        public override void _FixedUpdate() {
            if (Time.time - timeSinceShot < DURATION_SHOT)
            {
                MoveAndBounce();
            }
        }

        public override void OnExit() {
            ballSprite.localScale = new Vector3(1, 1, 1);
            shotParticles.Stop();
        }
    }
