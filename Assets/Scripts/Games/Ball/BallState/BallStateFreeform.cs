
    using System;
    using System.Collections;
    using UnityEngine;

    public class BallStateFreeform: BallState {

 

        public override  void OnEnter() {
            playerDetectArea.EnableDetection(false);
            ball.StartCoroutine(EnableDetectionWithDelay(stateData.LockDuration));
            GameInterface.Interface.EventSystem.Publish(new BallFreeformToLerpCameraOffsetEvent(true));
        }
        private IEnumerator EnableDetectionWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            playerDetectArea.EnableDetection(true);
        }

        public override void OnExit() {
            playerDetectArea.EnableDetection(false);
            GameInterface.Interface.EventSystem.Publish(new BallFreeformToLerpCameraOffsetEvent(false));
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
