
    using System;
    using System.Collections;
    using UnityEngine;

    public class BallViewStateFreeform: BallViewState {

        private float LockDurationCheckTimer = 0f;
        private bool CanCarried=false;
        

        public override  void OnEnter() {
            CanCarried=false;
            LockDurationCheckTimer = 0f;
            GameInterface.Interface.EventSystem.Publish(new BallFreeformToLerpCameraOffsetEvent(true));
        }

        public override void OnExit() {
            GameInterface.Interface.EventSystem.Publish(new BallFreeformToLerpCameraOffsetEvent(false));
        }

        public override bool CanCarriedBall() {
            return CanCarried;
        }


        public override void _Update() {
            SetBallAnimationFromVelocity();
            SetLockDuration();

            
        }
        private void SetLockDuration() {
            if (!CanCarried&&LockDurationCheckTimer < stateData.LockDuration) {
                LockDurationCheckTimer+= Time.deltaTime;
            }
            else {
                LockDurationCheckTimer = 0f;
                CanCarried = true;
            }
        }

        public override void _FixedUpdate() {
            MoveVertical(BOUNCINESS);
            MoveHorizontal();
        }

        public override bool CanAirInteract() {
            return true; 
        }

    }
