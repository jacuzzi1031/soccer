
    using UnityEngine;

    public class PlayerStateShooting: PlayerState {

        public override void OnEnter() {
            animator.Play("kick");
            
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
