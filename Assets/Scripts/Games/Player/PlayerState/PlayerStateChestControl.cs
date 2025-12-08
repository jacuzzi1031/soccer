
    using UnityEngine;

    public class PlayerStateChestControl: PlayerState {
        private const float DURATION_CONTROL = 0.5f;
        private float enterTime;
        public override void OnEnter() {
            animator.Play("chest_control");
            rb.velocity=Vector2.zero;
            enterTime = Time.time;
        }

        public override void _Update() {
            if (Time.time - enterTime > DURATION_CONTROL)
            {
                TransitionState(Player.State.MOVING);
            }
        }
    }
