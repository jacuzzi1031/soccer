
    using UnityEngine;

    public class PlayerViewViewStateRecovering: PlayerViewState {
        private const float DURATION_RECOVERY = 0.5f; 
        private float timeStartRecovery;
        public override void OnEnter()
        {
            timeStartRecovery = Time.time;

            PlayerView.rb.velocity = Vector2.zero;
            animator.Play("recover");
        }
        public override void _Update()
        {
            if (Time.time - timeStartRecovery > DURATION_RECOVERY)
            {
                TransitionState(PlayerView.State.MOVING);
            }
        }
    }
