using System;
using UnityEngine;

    public class BallStateCarried: BallState {
        private const float DRIBBLE_FREQUENCY = 10f;
        private const float DRIBBLE_INTENSITY = 0.167f;
        private readonly Vector2 OFFSET_FROM_PLAYER = new Vector2(0.60f, 0.22f);
        private const float minVelocityThreshold = 0.01f;
        private float dribbleTime = 0f;
        //Carried  
        public override void _Update()
        {
            if (carrier.rb.velocity !=Vector2.zero)
            {
                if (carrier.headingRight)
                {
                    animator.Play("roll");
                }
                else
                {
                    animator.Play("rollback");
                }
                
                //CameraYdamping
                float velocityY = rb.velocity.y;
                if (CameraManager.Instance.IsLerpingScreenY) return;
                if (Mathf.Abs(velocityY) > minVelocityThreshold)
                {
                    CameraManager.Instance.LerpScreenY(velocityY);
                }
                else {
                    CameraManager.Instance.LerpScreenY(0);
                }
            }
            else
            {
                animator.Play("idle");
            }
            
            // ProcessGravity();
        }

        public override void _FixedUpdate() {
            dribbleTime += Time.fixedDeltaTime;

            float vx = 0f;
            if (carrier.rb.velocity.x != 0)
                vx = Mathf.Cos(dribbleTime * DRIBBLE_FREQUENCY) * DRIBBLE_INTENSITY;

            bool facingRight = carrier.headingRight;
            ball.transform.position =
                (Vector2)carrier.transform.position +
                new Vector2((facingRight ? OFFSET_FROM_PLAYER.x : -OFFSET_FROM_PLAYER.x) + vx,
                    OFFSET_FROM_PLAYER.y);
        }

        public override void OnEnter() {
            Debug.Assert(carrier != null, "Carrier must not be null.");
            ball.velocity=Vector2.zero;
            ball.heightVelocity=0.0f;
            ball.height = 0f;
        }
        
        public override void OnExit()
        {
            // GameEvents.BallReleased?.Invoke();
        }
    }
