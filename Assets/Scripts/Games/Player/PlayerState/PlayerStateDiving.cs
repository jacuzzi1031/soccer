
    using UnityEngine;

    public class PlayerStateDiving: PlayerState {
        public float DURATION_DIVE = 0.5f;
        public Vector2 moveDir;
        public float timeStartDive;
        public PlayerStateDiving(Sprite sprite) {
            playStyleSprite=sprite;
        }

        public override void OnEnter() {
            Vector2 targetDive = new Vector2(player.spawnPosition.x, ball.transform.position.y);
            moveDir = (targetDive - (Vector2)player.transform.position).normalized;
            if (moveDir.y > 0)
                animator.Play("dive_up");
            else
                animator.Play("dive_down");
            // Record start time
            timeStartDive = Time.time;
            //是扑到才event publish
            GameInterface.Interface.EventSystem.Publish(new PlayStyleShowEvent(player.playerId,playStyleSprite));
        }

        public override void _FixedUpdate() {
            player.rb.velocity = moveDir * player.speed;
        }

        public override void _Update() {
            if (Time.time - timeStartDive > DURATION_DIVE)
            {
                TransitionState(Player.State.RECOVERING);
            }
        }
    }
