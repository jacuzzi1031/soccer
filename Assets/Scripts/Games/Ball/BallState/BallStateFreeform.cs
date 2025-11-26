
    using System;
    using UnityEngine;

    public class BallStateFreeform: BallState {

 
        private const float MAX_CAPTURE_HEIGHT = 1.39f;
        public override  void OnEnter() {
            playerDetectArea.OnTriggered+= PlayerDetectAreaOnOnEnter;
            GameInterface.Interface.EventSystem.Publish(new BallFreeformToLerpCameraOffsetEvent(true));
        }

        public override void OnExit() {
            playerDetectArea.OnTriggered-= PlayerDetectAreaOnOnEnter;
            GameInterface.Interface.EventSystem.Publish(new BallFreeformToLerpCameraOffsetEvent(false));
        }
        private void PlayerDetectAreaOnOnEnter(Collider2D obj) {
            Player body = obj.GetComponentInParent<Player>();
            if (!body) return;

            if (body.CanCarryBall() && ball.height < MAX_CAPTURE_HEIGHT)
            {
                ball.carrier = body;
                body.ControlBall();
                TransitionState(Ball.State.CARRIED);
            }
        }

        public override void _Update() {
            SetBallAnimationFromVelocity();
        }

        public override void _FixedUpdate() {
            float delta = Time.fixedDeltaTime;//0.02s
            float friction = ball.height > 0 ? ball.frictionAir : ball.frictionGround;
            ball.velocity = Vector2.MoveTowards(ball.velocity, Vector2.zero, friction * delta);
            ApplyGravity(ball.BOUNCINESS);
            MoveAndBounce();
        }

        public override bool CanAirInteract() {
            return true; 
        }

    }
