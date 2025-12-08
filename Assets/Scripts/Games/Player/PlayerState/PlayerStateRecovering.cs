
    using UnityEngine;

    public class PlayerStateRecovering: PlayerState {
        private const float DURATION_RECOVERY = 0.5f; 
        private float timeStartRecovery;
        public override void OnEnter()
        {
            timeStartRecovery = Time.time;

            player.rb.velocity = Vector2.zero;
            animator.Play("recover");
        }
        public override void _Update()
        {
            if (Time.time - timeStartRecovery > DURATION_RECOVERY)
            {
                TransitionState(Player.State.MOVING);
            }
        }
    }
