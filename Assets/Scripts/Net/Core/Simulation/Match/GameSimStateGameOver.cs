
    using UnityEngine.iOS;

    public class GameSimStateGameOver: GameSimState {
        public override void OnEnter() {
            if (stateData.scoringIsHome) {
                _matchSystem.goalsHome++;
            }
            else {
                _matchSystem.goalsAway++;
            }
            bool winnerIsHome = _matchSystem.getWinnerIsHome();
            _commandBuffer.Enqueue(new SimulationCommand
            {
                Type = SimulationCommandType.GameOverWinner,
                isHome = winnerIsHome
            });
            _eventBus.Publish(new GameOverSignal());
        }
    }
