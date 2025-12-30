
    using UnityEngine;

    public class PlayerStateShooting: PlayerState {

        public override void OnEnter() {
            animator.Play("kick");
            if (stateData.isPowerShot) {
                SoundManager.Instance.Play(SoundManager.Instance.audioRefs.POWERSHOT);
            }
            else {
                SoundManager.Instance.Play(SoundManager.Instance.audioRefs.SHOT);
            }
        }



        public override void OnAnimationComplete() {
            if (player.controlScheme == Player.ControlScheme.CPU) {
                TransitionState(Player.State.RECOVERING);
            }
            else {
                TransitionState(Player.State.MOVING);
            }

            ShootBall();
        }

        public void ShootBall() {
            ball.shoot(stateData.ShotDirection * stateData.ShotPower);
        }
    }
