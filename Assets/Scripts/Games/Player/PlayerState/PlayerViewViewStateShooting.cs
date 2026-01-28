
    using UnityEngine;

    public class PlayerViewViewStateShooting: PlayerViewState {

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
            if (PlayerView.controlScheme == ControlScheme.CPU) {
                TransitionState(PlayerView.State.RECOVERING);
            }
            else {
                TransitionState(PlayerView.State.MOVING);
            }

            ShootBall();
        }

        public void ShootBall() {
            BallView.shoot(stateData.ShotDirection * stateData.ShotPower);
        }
    }
