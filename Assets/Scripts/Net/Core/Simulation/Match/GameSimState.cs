
    using Unity.VisualScripting;

    public class GameSimState {
        protected MatchSystem _matchSystem;
        protected SimEventBus _eventBus;
        protected CommandBuffer _commandBuffer;
        protected GameStateData stateData;
        public void Setup(MatchSystem matchSystem, GameStateData contextData,SimEventBus eventBus,CommandBuffer commandBuffer)
        {
            _matchSystem=matchSystem;
            stateData = contextData;
            _eventBus=eventBus;
            _commandBuffer = commandBuffer;
        }
        public virtual void OnEnter() {
        
        }
        public virtual void OnExit() {
        
        }
        public virtual void _Update(float deltaTime) {
            
        }

        public virtual void OnKickoffStart(int seatIndex) {
        }

        public virtual void OnTeamScored(OnTeamScoredEvent obj) {
        }
    }
