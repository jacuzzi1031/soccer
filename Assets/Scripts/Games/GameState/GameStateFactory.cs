using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStateFactory
{
    private void Awake() => Debug.Log("test！！！！！");
    private Dictionary<MatchSystem.State, Func<GameState>> states;

    public GameStateFactory()
    {   
        states = new Dictionary<MatchSystem.State, Func<GameState>>
        {
            { MatchSystem.State.GAMEOVER, () => new GameStateGameOver() },
            { MatchSystem.State.IN_PLAY, () => new GameStateInPlay() },
            { MatchSystem.State.KICKOFF, () => new GameStateKickoff() },
            { MatchSystem.State.OVERTIME, () => new GameStateOvertime() },
            { MatchSystem.State.RESET, () => new GameStateReset() },
            { MatchSystem.State.SCORED, () => new GameStateScored() },
        };
    }
    public GameState GetFreshState(MatchSystem.State state)
    {
        if (states.TryGetValue(state, out var factory)) {
            return factory();
        }
        throw new ArgumentException($"GameState not registered: {state}");
    }
}