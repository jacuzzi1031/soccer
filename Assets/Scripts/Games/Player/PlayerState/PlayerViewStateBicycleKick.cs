
    using UnityEngine;

    public class PlayerViewStateBicycleKick: PlayerViewState {
        
        public const float BALL_BICYCLEKICK_MAX = 32f;
        public const float BONUS_POWER =1.2f;
        
        public PlayerViewStateBicycleKick(Sprite sprite)
        {
            playStyleSprite=sprite;
        }
        public override void OnEnter() {
            animator.Play("bicycle_kick");
            SoundManager.Instance.Play(SoundManager.Instance.audioRefs.POWERSHOT);
        }

        public override void VolleyShot(BallView body) {
            if (body.height < BALL_BICYCLEKICK_MAX) {
                Vector2 destination = targetGoal.GetRandomTargetPosition();
                Vector2 direction = (destination - (Vector2)body.transform.position).normalized;
                body.shoot(direction * PlayerView.power * BONUS_POWER);
                GameInterface.Interface.EventSystem.Publish(new PlayStyleShowEvent(PlayerView.playerId,playStyleSprite));
            }
        }
        public override bool CanVolleyKickOrHeader() {
            return true;
        }

        public override void OnAnimationComplete() {
            TransitionState(PlayerView.State.RECOVERING);
        }
    }
