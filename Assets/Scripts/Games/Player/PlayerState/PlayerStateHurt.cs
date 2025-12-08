
    using UnityEngine;

    public class PlayerStateHurt: PlayerState {
        private const float AIR_FRICTION = 35f;
        private const float BALL_TUMBLE_SPEED = 100f;
        private const float DURATION_HURT = 1f;
        private const float HURT_HEIGHT_VELOCITY = 3f;

        private float startHurtTime;

        public override void OnEnter() {
            animator.Play("hurt");
            startHurtTime = Time.time;
            player.heightVelocity = HURT_HEIGHT_VELOCITY;
            player.height = 0.1f;
            Vector2 tumbleDir = stateData.MoveDir;
            ball.Tumble(tumbleDir * BALL_TUMBLE_SPEED);
        }

        public override void _Update() {
            if ((Time.time ) - startHurtTime > DURATION_HURT)
            {
                TransitionState(Player.State.RECOVERING);
            }
            rb.velocity = Vector2.MoveTowards(
                rb.velocity,
                Vector2.zero,
                Time.deltaTime * AIR_FRICTION
            );
        }
    }
