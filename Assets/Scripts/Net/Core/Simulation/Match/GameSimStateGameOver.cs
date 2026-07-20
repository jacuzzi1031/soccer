
    public class GameSimStateGameOver: GameSimState {
        public override void OnEnter() {
            bool winnerIsHome = _matchSystem.getWinnerIsHome();
            _commandBuffer.Enqueue(new SimulationCommand
            {
                Type = SimulationCommandType.GameOverWinner,
                isHome = winnerIsHome
            });
            _eventBus.Publish(new GameOverSignal());
        }
    }
