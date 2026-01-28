
    using UnityEngine;

    public class PlayerViewViewStateVolleyKickOrHeader: PlayerViewState {
        // public const float BALL_HEIGHT_MIN = 1f;
        public const float BALL_VOLLEYKICK_MIN = 8f;
        public const float BALL_VOLLEYKICK_MAX = 35f;
        public const float HEIGHT_START = 0.1f;
        public const float HEIGHT_VELOCITY_START = 1.5f;
        public bool isHeader = false;
        public const float BONUS_POWER =1.8f;
        public Sprite volleyKickSprite;
        public Sprite headerSprite;

        public PlayerViewViewStateVolleyKickOrHeader(Sprite volleyKickSprite, Sprite headerSprite) {
            this.volleyKickSprite=volleyKickSprite;
            this.headerSprite=headerSprite;
        }
        public override void OnEnter() {
            if (BallView.height < BALL_VOLLEYKICK_MIN) {
                animator.Play("volley_kick");
                isHeader = false;
            }
            else {
                PlayerView.height = HEIGHT_START;
                PlayerView.heightVelocity = HEIGHT_VELOCITY_START;
                animator.Play("header");
                isHeader = true;
            }
        }

        public override void VolleyShot(BallView body) {
            if (body.height < BALL_VOLLEYKICK_MAX) {
                Vector2 destination = targetGoal.GetRandomTargetPosition();
                Vector2 direction = (destination - (Vector2)body.transform.position).normalized;
                body.shoot(direction * PlayerView.power * BONUS_POWER);
                SoundManager.Instance.Play(SoundManager.Instance.audioRefs.POWERSHOT);

                if (isHeader) {
                    GameInterface.Interface.EventSystem.Publish(new PlayStyleShowEvent(PlayerView.playerId,headerSprite));
                }
                else {
                    GameInterface.Interface.EventSystem.Publish(new PlayStyleShowEvent(PlayerView.playerId,volleyKickSprite));
                }
            }
        }

        public override bool CanVolleyKickOrHeader() {
            return true;
        }

        public override void _Update() {
            if (isHeader&&PlayerView.height<0.01f) {
                TransitionState(PlayerView.State.RECOVERING);
            }
        }

        public override void OnAnimationComplete() {
            if (!isHeader) {
                TransitionState(PlayerView.State.RECOVERING);
            }
        }
    }
