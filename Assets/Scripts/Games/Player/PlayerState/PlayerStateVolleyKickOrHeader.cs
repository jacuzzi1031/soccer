
    using UnityEngine;

    public class PlayerStateVolleyKickOrHeader: PlayerState {
        // public const float BALL_HEIGHT_MIN = 1f;
        public const float BALL_VOLLEYKICK_MIN = 8f;
        public const float BALL_VOLLEYKICK_MAX = 35f;
        public const float HEIGHT_START = 0.1f;
        public const float HEIGHT_VELOCITY_START = 1.5f;
        public bool isHeader = false;
        public const float BONUS_POWER =1.8f;
        public Sprite volleyKickSprite;
        public Sprite headerSprite;

        public PlayerStateVolleyKickOrHeader(Sprite volleyKickSprite, Sprite headerSprite) {
            this.volleyKickSprite=volleyKickSprite;
            this.headerSprite=headerSprite;
        }
        public override void OnEnter() {
            if (ball.height < BALL_VOLLEYKICK_MIN) {
                animator.Play("volley_kick");
                isHeader = false;
            }
            else {
                player.height = HEIGHT_START;
                player.heightVelocity = HEIGHT_VELOCITY_START;
                animator.Play("header");
                isHeader = true;
                
                
            }
        }

        public override void VolleyShot(Ball body) {
            if (body.height < BALL_VOLLEYKICK_MAX) {
                Vector2 destination = targetGoal.GetRandomTargetPosition();
                Vector2 direction = (destination - (Vector2)body.transform.position).normalized;
                body.shoot(direction * player.power * BONUS_POWER);

                if (isHeader) {
                    GameInterface.Interface.EventSystem.Publish(new PlayStyleShowEvent(player.playerId,headerSprite));
                }
                else {
                    GameInterface.Interface.EventSystem.Publish(new PlayStyleShowEvent(player.playerId,volleyKickSprite));
                }
            }
        }

        public override bool CanVolleyKickOrHeader() {
            return true;
        }

        public override void _Update() {
            if (isHeader&&player.height<0.01f) {
                TransitionState(Player.State.RECOVERING);
            }
        }

        public override void OnAnimationComplete() {
            if (!isHeader) {
                TransitionState(Player.State.RECOVERING);
            }
        }
    }
