using System;
using System.Collections.Generic;

public class GameSimStateFactory
{
    private Dictionary<MatchState, Func<GameSimState>> states;

    public GameSimStateFactory()
    {   
        states = new Dictionary<MatchState, Func<GameSimState>>
        {
            { MatchState.GAMEOVER, () => new GameSimStateGameOver() },
            { MatchState.IN_PLAY, () => new GameSimStateInPlay() },
            { MatchState.KICKOFF, () => new GameSimStateKickoff() },
            { MatchState.OVERTIME, () => new GameSimStateOvertime() },
            { MatchState.RESET, () => new GameSimStateReset() },
            { MatchState.SCORED, () => new GameSimStateScored() },
        };
    }
    public GameSimState GetFreshState(MatchState state)
    {
        if (states.TryGetValue(state, out var factory)) {
            return factory();
        }
        throw new ArgumentException($"GameState not registered: {state}");
    }
}