
    using System;
    using GameFrameSync;
    using UnityEngine;

    public class PlayerViewViewStateMoving: PlayerViewState {
        
        private Vector2 moveDir;

        private float goalieSpeed = 44f;

        private float movingSpeed;
        
        public override void OnEnter() {
            animator.Play("movement");
            movingSpeed=PlayerView.speed;
        }


            public override void OnShoot() {
                
                if (PlayerView.HasBall())
                {
                    TransitionState(PlayerView.State.PREPPING_SHOT);
                }
                else if (BallView.CanAirInteract())
                {
                    if (PlayerView.IsFacingTargetGoal())
                    {
                        TransitionState(PlayerView.State.VOLLEY_KICK_OR_HEADER);
                    }
                    else
                    {
                        TransitionState(PlayerView.State.BICYCLE_KICK);
                    }
                }
            }

        public override void OnPass(InputType passType) {
            if (PlayerView.HasBall()) {
                TransitionState(PlayerView.State.PASSING,PlayerStateData.Build().SetInputType(passType).SetMoveDir(moveDir));
            }
            else {
                //tackle
                TransitionState(PlayerView.State.TACKLING,PlayerStateData.Build().SetMoveDir(moveDir));
            }
            
        }

        public override void _Update() {
            if (PlayerView.controlScheme == ControlScheme.CPU) {
                aiBehavior.UpdateAI();                  
                moveDir = aiBehavior.GetAIMoveDir();  
            }
            else {
                // moveDir = GameInput.Instance.GetMovementVectorNormalized();
            }

            SetMovementAnimation();
        }
        public override void _FixedUpdate() {
            if (PlayerView.role != Role.GOALIE) {
                rb.velocity = moveDir * movingSpeed;
            }
            else {
                rb.velocity = moveDir*goalieSpeed;
            }
        }
        


        public override bool CanCarryBall() {
            return PlayerView.role != Role.GOALIE;
        }

        public override void OnExit() {
            movingSpeed = 0f;
            base.OnExit();
        }
    }
