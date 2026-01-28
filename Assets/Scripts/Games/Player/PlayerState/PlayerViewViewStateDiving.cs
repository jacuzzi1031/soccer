
    using UnityEngine;

    public class PlayerViewViewStateDiving: PlayerViewState {
        public float DURATION_DIVE = 0.5f;
        public Vector2 moveDir;
        public float timeStartDive;
        public PlayerViewViewStateDiving(Sprite sprite) {
            playStyleSprite=sprite;
        }

        public override void OnEnter() {
            Vector2 targetDive = new Vector2(PlayerView.spawnPosition.x, BallView.transform.position.y);
            moveDir = (targetDive - (Vector2)PlayerView.transform.position).normalized;
            if (moveDir.y > 0)
                animator.Play("dive_up");
            else
                animator.Play("dive_down");
            // Record start time
            timeStartDive = Time.time;
            //是扑到才event publish
            GameInterface.Interface.EventSystem.Publish(new PlayStyleShowEvent(PlayerView.playerId,playStyleSprite));
        }

        public override void _FixedUpdate() {
            PlayerView.rb.velocity = moveDir * PlayerView.speed;
        }

        public override void _Update() {
            if (Time.time - timeStartDive > DURATION_DIVE)
            {
                TransitionState(PlayerView.State.RECOVERING);
            }
        }
    }
