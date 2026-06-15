
    using Net.Core.Simulation.SimSignal;

    public class GameSimStateScored:GameSimState {
        private int _elapsedFrames;
        private const int CELEBRATION_FRAMES = 180; 

        public override void OnEnter()
        {
            if (stateData.scoringIsHome)
            {
                _matchSystem.goalsHome++;
            }
            else
            {
                _matchSystem.goalsAway++;
            }

            _elapsedFrames = 0;
            _eventBus.Publish(new OnScoreChangedSignal());
        }

        public override void _Update()
        {
            _elapsedFrames++;

            if (_elapsedFrames >= CELEBRATION_FRAMES)
            {
                _matchSystem.SwitchGameState(MatchState.RESET, stateData);
            }
        }
    }
