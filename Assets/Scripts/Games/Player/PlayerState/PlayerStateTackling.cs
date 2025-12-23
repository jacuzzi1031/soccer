
    using UnityEngine;

    public class PlayerStateTackling : PlayerState{
        public bool isTackleComplete=false;
        public const float GROUND_FRICTION = 120f;
        public const float DURATION_PRIOR_RECOVERY = 0.2f;
        public Vector2 moveDir;
        public float timeFinishTackle;
        public PlayerStateTackling(Sprite sprite) {
            playStyleSprite=sprite;
        }

        public override void OnEnter() {
            tackleDamageEmitterArea.enabled = true;
            animator.Play("tackle");
            if (stateData.MoveDir.sqrMagnitude <= 0.01f) moveDir = player.headingRight ? Vector2.right : Vector2.left;
            else moveDir = stateData.MoveDir.normalized;
        }

        public override void OnExit() {
            tackleDamageEmitterArea.enabled = false;
            base.OnExit();
        }

        public override void _FixedUpdate() {
            if (!isTackleComplete) {
                float dt = Time.fixedDeltaTime;
                rb.velocity = Vector2.MoveTowards(rb.velocity, Vector2.zero, GROUND_FRICTION* dt);
                if (rb.velocity.magnitude < 0.01f) {
                    isTackleComplete=true;
                    timeFinishTackle = Time.time;
                }

            }
            else if(Time.time-timeFinishTackle>DURATION_PRIOR_RECOVERY) {
                TransitionState(Player.State.RECOVERING);
            }
        }
    }
