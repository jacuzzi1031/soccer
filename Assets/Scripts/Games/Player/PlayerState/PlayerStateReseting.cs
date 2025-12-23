
    using UnityEngine;

    public class PlayerStateReseting: PlayerState {
        public bool hasArrived = false;
        public float arriveDistance = 2f;
        public override void OnEnter() {
            GameInterface.Interface.EventSystem.Subscribe<OnKickoffStartedEvent>(OnKickoffStarted);
        }

        public override void OnExit() {
            GameInterface.Interface.EventSystem.Unsubscribe<OnKickoffStartedEvent>(OnKickoffStarted);
        }
        private void OnDestroy() {
            GameInterface.Interface.EventSystem.Unsubscribe<OnKickoffStartedEvent>(OnKickoffStarted);
        }
        public override void _Update()
        {
            if (hasArrived) return;

            Vector2 currentPos = player.transform.position;
            Vector2 direction = (stateData.ResetPosition - currentPos).normalized;

            if ((stateData.ResetPosition - currentPos).sqrMagnitude < arriveDistance * arriveDistance)
            {
                hasArrived = true;
                player.rb.velocity = Vector2.zero;
                player.FaceTowardsTargetGoal();
            }
            else
            {
                player.rb.velocity = direction * player.speed;
            }

            SetMovementAnimation();
            player.FlipSprite();
        }

        public override bool IsReadyForKickoff()
        {
            return hasArrived;
        }

        private void OnKickoffStarted(OnKickoffStartedEvent obj) {
            TransitionState(Player.State.MOVING);
        }
    }
