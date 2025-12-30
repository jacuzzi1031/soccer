
    using UnityEngine;

    public class PlayerStateBicycleKick: PlayerState {
        
        public const float BALL_BICYCLEKICK_MAX = 32f;
        public const float BONUS_POWER =1.2f;
        
        public PlayerStateBicycleKick(Sprite sprite)
        {
            playStyleSprite=sprite;
        }
        public override void OnEnter() {
            animator.Play("bicycle_kick");
            SoundManager.Instance.Play(SoundManager.Instance.audioRefs.POWERSHOT);
        }

        public override void VolleyShot(Ball body) {
            if (body.height < BALL_BICYCLEKICK_MAX) {
                Vector2 destination = targetGoal.GetRandomTargetPosition();
                Vector2 direction = (destination - (Vector2)body.transform.position).normalized;
                body.shoot(direction * player.power * BONUS_POWER);
                GameInterface.Interface.EventSystem.Publish(new PlayStyleShowEvent(player.playerId,playStyleSprite));
            }
        }
        public override bool CanVolleyKickOrHeader() {
            return true;
        }

        public override void OnAnimationComplete() {
            TransitionState(Player.State.RECOVERING);
        }
    }
