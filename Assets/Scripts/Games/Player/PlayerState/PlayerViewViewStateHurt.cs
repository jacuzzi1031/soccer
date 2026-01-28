
    using UnityEngine;

    public class PlayerViewViewStateHurt: PlayerViewState {
        private const float AIR_FRICTION = 35f;
        private const float BALL_TUMBLE_SPEED = 100f;
        private const float DURATION_HURT = 1f;
        private const float HURT_HEIGHT_VELOCITY = 3f;

        private float startHurtTime;

        public override void OnEnter() {
            animator.Play("hurt");
            startHurtTime = Time.time;
            PlayerView.heightVelocity = HURT_HEIGHT_VELOCITY;
            PlayerView.height = 0.1f;
            Vector2 tumbleDir = stateData.MoveDir;
            BallView.Tumble(tumbleDir * BALL_TUMBLE_SPEED);
            SoundManager.Instance.Play(SoundManager.Instance.audioRefs.HURT);
        }

        public override void _Update() {
            if ((Time.time ) - startHurtTime > DURATION_HURT)
            {
                TransitionState(PlayerView.State.RECOVERING);
            }
            rb.velocity = Vector2.MoveTowards(
                rb.velocity,
                Vector2.zero,
                Time.deltaTime * AIR_FRICTION
            );
        }
    }
