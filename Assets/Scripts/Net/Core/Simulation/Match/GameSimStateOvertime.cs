
    public class GameSimStateOvertime: GameSimState {
        public override void OnTeamScoring(bool scoringIsHome) {
            if (scoringIsHome)
            {
                _matchSystem.goalsHome++;
            }
            else
            {
                _matchSystem.goalsAway++;
            }
            _matchSystem.SwitchGameState(MatchState.GAMEOVER,GameStateData.Build().SetIsHomeScoring(scoringIsHome));
        }
    }
