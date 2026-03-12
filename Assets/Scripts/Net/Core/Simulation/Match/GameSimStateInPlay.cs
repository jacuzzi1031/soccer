
    public class GameSimStateInPlay: GameSimState {
        public override void _Update(float deltaTime) {

            _matchSystem.timeLeft -= deltaTime;

            if (_matchSystem.IsTimeUp())
            {
                if (_matchSystem.IsTied())
                {
                    _matchSystem.SwitchGameState(MatchState.OVERTIME);
                }
                else
                {
                    bool isHome=_matchSystem.goalsHome>_matchSystem.goalsAway;
                    _matchSystem.SwitchGameState(MatchState.GAMEOVER,GameStateData.Build().SetIsHomeScoring(isHome));
                }
            }
        }
        public override void OnTeamScoring(bool isHome) {
            _matchSystem.SwitchGameState(MatchState.SCORED,GameStateData.Build().SetIsHomeScoring(isHome));
        }
    }
