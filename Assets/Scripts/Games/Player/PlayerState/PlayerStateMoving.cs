
    using System;
    using UnityEngine;

    public class PlayerStateMoving: PlayerState {
        
        private Vector2 moveDir;
        private float runThreshold = 45f;
        private float goalieSpeed = 44f;
        private int speedHash;
        private float movingSpeed;
        
        public override void OnEnter() {
            animator.Play("movement");
            speedHash = Animator.StringToHash("Speed");
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
        
        public void SetMovementAnimation() {
            animator.SetFloat(speedHash, rb.velocity.magnitude);
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

            if (!info.IsName("movement"))
            {
                if (runParticles.isPlaying)
                    runParticles.Stop();
                return;
            }
            float speed = animator.GetFloat(speedHash);
            if (speed >= runThreshold)
            {
                if (!runParticles.isPlaying)
                    runParticles.Play();
            }
            else
            {
                if (runParticles.isPlaying)
                    runParticles.Stop();
            }
        }

        public override bool CanCarryBall() {
            return player.role != Player.Role.GOALIE;
        }

        public override void OnExit() {
            movingSpeed = 0f;
        }
    }
