
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
                    _matchSystem.SwitchGameState(MatchState.GAMEOVER);
                }
            }
        }
        public override void OnTeamScored(OnTeamScoredEvent obj) {
            _matchSystem.SwitchGameState(MatchState.SCORED,GameStateData.Build().SetCountryScoredOn(obj.CountryScoredOn));
        }
    }
