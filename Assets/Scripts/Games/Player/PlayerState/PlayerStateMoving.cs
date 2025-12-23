
    using System;
    using UnityEngine;

    public class PlayerStateMoving: PlayerState {
        
        private Vector2 moveDir;

        private float goalieSpeed = 44f;

        private float movingSpeed;
        
        public override void OnEnter() {
            animator.Play("movement");
            movingSpeed=player.speed;
        }


        public override void OnShoot() {
            
            if (player.HasBall())
            {
                TransitionState(Player.State.PREPPING_SHOT);
            }
            else if (ball.CanAirInteract())
            {
                if (player.IsFacingTargetGoal())
                {
                    TransitionState(Player.State.VOLLEY_KICK_OR_HEADER);
                }
                else
                {
                    TransitionState(Player.State.BICYCLE_KICK);
                }
            }
        }

        public override void OnPass(GameInput.PlayerInputType passType) {
            if (player.HasBall()) {
                TransitionState(Player.State.PASSING,PlayerStateData.Build().SetInputType(passType).SetMoveDir(moveDir));
            }
            else {
                //tackle
                TransitionState(Player.State.TACKLING,PlayerStateData.Build().SetMoveDir(moveDir));
            }
            
        }

        public override void _Update() {
            if (player.controlScheme == Player.ControlScheme.CPU) {
                aiBehavior.UpdateAI();                  
                moveDir = aiBehavior.GetAIMoveDir();  
            }
            else {
                moveDir = GameInput.Instance.GetMovementVectorNormalized();
            }

            SetMovementAnimation();
        }
        public override void _FixedUpdate() {
            if (player.role != Player.Role.GOALIE) {
                rb.velocity = moveDir * movingSpeed;
            }
            else {
                rb.velocity = moveDir*goalieSpeed;
            }

        }
        


        public override bool CanCarryBall() {
            return player.role != Player.Role.GOALIE;
        }

        public override void OnExit() {
            movingSpeed = 0f;
            base.OnExit();
        }
    }
