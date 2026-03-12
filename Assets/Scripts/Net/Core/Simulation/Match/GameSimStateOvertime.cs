
    public class GameSimStateOvertime: GameSimState {
        public override void OnTeamScoring(bool scoringIsHome) {
            _matchSystem.SwitchGameState(MatchState.GAMEOVER,GameStateData.Build().SetIsHomeScoring(scoringIsHome));
        }
    }
