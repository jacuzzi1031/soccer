using System;
using UnityEngine;

    public class BallStateCarried: BallState {
        private const float DRIBBLE_FREQUENCY = 10f;
        private const float DRIBBLE_INTENSITY = 3f;
        private readonly Vector2 OFFSET_FROM_PLAYER = new Vector2(10.8f, 4f);
        private const float minVelocityThreshold = 0.01f;
        private float dribbleTime = 0f;
        //Carried  
        private float cameraCheckTimer = 0f;
        public float cameraCheckInterval = 0.2f;

        public override void _Update()
        {   
            Vector2 currentVelocity = carrier.rb.velocity;
            if (currentVelocity != Vector2.zero)
            {
                animator.Play(carrier.headingRight ? "roll" : "rollback");

                cameraCheckTimer += Time.deltaTime;
                if (cameraCheckTimer > cameraCheckInterval)
                {
                    cameraCheckTimer = 0f;
                    CheckCameraY(currentVelocity.y);
                }
            }
            else
            {
                animator.Play("idle");
            }
        }

        void CheckCameraY(float velocityY)
        {
            if (CameraManager.Instance.IsLerpingScreenY) return;

            if (Mathf.Abs(velocityY) > minVelocityThreshold)
                CameraManager.Instance.LerpScreenY(velocityY);
            else
                CameraManager.Instance.LerpScreenY(0);
        }

        public override void _FixedUpdate() {
            dribbleTime += Time.fixedDeltaTime;

            float vx = 0f;
            if (carrier.rb.velocity.x != 0)
                vx = Mathf.Cos(dribbleTime * DRIBBLE_FREQUENCY) * DRIBBLE_INTENSITY;

            bool facingRight = carrier.headingRight;
            Vector2 targetPos  =
                (Vector2)carrier.transform.position +
                new Vector2((facingRight ? OFFSET_FROM_PLAYER.x : -OFFSET_FROM_PLAYER.x) + vx,
                    OFFSET_FROM_PLAYER.y);
            rb.MovePosition(targetPos);
            MoveVertical();
        }

        public override void OnEnter() {
            Debug.Assert(carrier != null, "Carrier must not be null.");
            ball.velocity=Vector2.zero;
            ball.heightVelocity=0.0f;
            ball.height = 0f;
            if (carrier.controlScheme == Player.ControlScheme.CPU) {
                GameInterface.Interface.EventSystem.Publish(new PlayerBecomesCarrierEvent(carrier.playerId));
            }
            GameInterface.Interface.EventSystem.Publish(new OnBallPossessedEvent(carrier.fullName));
        }
        
        public override void OnExit()
        {
            GameInterface.Interface.EventSystem.Publish(new OnBallReleasedEvent());
        }
    }
