
    using Net.Core.Simulation.SimSignal;

    public class GameSimStateScored:GameSimState {
        private float _elapsedTicks;
        private float DURATION_CELEBRATION= 3f;
        public override void OnEnter() {
            if (stateData.scoringIsHome) {
                _matchSystem.goalsHome++;
            }
            else {
                _matchSystem.goalsAway++;
            }

            _elapsedTicks = 0f;
            _eventBus.Publish(new OnScoreChangedSignal());
        }
        
        
        public override void _Update(float deltaTime) {
            _elapsedTicks+=deltaTime;

            if (_elapsedTicks >= DURATION_CELEBRATION)
            {
                _matchSystem.SwitchGameState(MatchState.RESET,stateData);
            }
        }
    }
