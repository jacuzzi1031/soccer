
    public class PlayerStateReseting: PlayerState {
        public override void OnEnter() {
            GameInterface.Interface.EventSystem.Publish(new TeamResetEvent());
            GameInterface.Interface.EventSystem.Subscribe<OnKickoffStartedEvent>(OnKickoffStarted);
        }

        private void OnKickoffStarted(OnKickoffStartedEvent obj) {
            TransitionState(Player.State.MOVING);
        }
    }
