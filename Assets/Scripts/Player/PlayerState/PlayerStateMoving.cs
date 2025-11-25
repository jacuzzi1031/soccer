
    using System;
    using UnityEngine;

    public class PlayerStateMoving: PlayerState {
        
        private Vector2 moveDir;
        private float runThreshold = 2.5f;
        private int speedHash;
        public override void OnEnter() {
            animator.Play("movement");
            speedHash = Animator.StringToHash("Speed");
            GameInput.Instance.OnShootAction+= InstanceOnOnShootAction;
        }

        private void InstanceOnOnSwapAction(object sender, EventArgs e) {
            if (!player.HasBall()) {
                player.Swap();
            }
        }

        private void InstanceOnOnShootAction(object sender, EventArgs e) {
            if (player.HasBall())
            {
                TransitionState(Player.State.PREPPING_SHOT);
            }
            else if (ball.CanAirInteract())
            {
                if (player.rb.velocity != Vector2.zero)
                {
                    if (player.IsFacingTargetGoal())
                    {
                        TransitionState(Player.State.VOLLEY_KICK);
                    }
                    else
                    {
                        TransitionState(Player.State.BICYCLE_KICK);
                    }
                }
                else
                {
                    TransitionState(Player.State.HEADER);
                }
            }
        }

        public override void _FixedUpdate() {
            // Transform.position +=或者rb.MovePosition(rb.position + ) 直接赋值位置 绕过 Rigidbody2D 的物理计算不碰撞
            rb.velocity = moveDir * player.speed;
        }

        public override void _Update() {
            if (player.controlScheme == Player.ControlScheme.CPU) {
                //ai
            }
            else {
                HandleMovement();
            }
            SetMovementAnimation();
        }

        private void HandleMovement() {
            moveDir = GameInput.Instance.GetMovementVectorNormalized();
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
        
    }
