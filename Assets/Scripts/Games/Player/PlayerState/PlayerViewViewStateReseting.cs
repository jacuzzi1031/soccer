
    using UnityEngine;

    public class PlayerViewViewStateReseting: PlayerViewState {
        public bool hasArrived = false;
        public float arriveDistance = 2f;
        public override void OnEnter() {
            GameInterface.Interface.EventSystem.Subscribe<OnKickoffStartedEvent>(OnKickoffStarted);
            animator.Play("movement");
        }

        public override void OnExit() {
            GameInterface.Interface.EventSystem.Unsubscribe<OnKickoffStartedEvent>(OnKickoffStarted);
        }
        public override void _Update()
        {
            if (hasArrived) return;

            Vector2 currentPos = PlayerView.transform.position;
            Vector2 direction = (stateData.ResetPosition - currentPos).normalized;

            if ((stateData.ResetPosition - currentPos).sqrMagnitude < arriveDistance * arriveDistance)
            {
                hasArrived = true;
                PlayerView.rb.velocity = Vector2.zero;
                PlayerView.FaceTowardsTargetGoal();
            }
            else
            {
                PlayerView.rb.velocity = direction * PlayerView.speed;
            }

            SetMovementAnimation();
            PlayerView.FlipSprite();
        }

        public override bool IsReadyForKickoff()
        {
            return hasArrived;
        }

        private void OnKickoffStarted(OnKickoffStartedEvent obj) {
            TransitionState(PlayerView.State.MOVING);
        }
    }
