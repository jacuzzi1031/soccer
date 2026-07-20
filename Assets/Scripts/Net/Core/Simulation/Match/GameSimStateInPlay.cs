
    public class GameSimStateInPlay: GameSimState {
        public override void _Update() {

            _matchSystem.framesLeft--;

            if (_matchSystem.IsTimeUp())
            {
                if (_matchSystem.IsTied())
                {
                    _matchSystem.SwitchGameState(MatchState.OVERTIME);
                }
                else
                {
                    _matchSystem.SwitchGameState(MatchState.GAMEOVER);
                }
            }
        }
        public override void OnTeamScoring(bool isHome) {
            _matchSystem.SwitchGameState(MatchState.SCORED,GameStateData.Build().SetIsHomeScoring(isHome));
        }
    }
