
    using UnityEngine;

    public class PlayerViewViewStateChestControl: PlayerViewState {
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
                TransitionState(PlayerView.State.MOVING);
            }
        }
    }
