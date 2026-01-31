using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStateFactory
{
    private void Awake() => Debug.Log("test！！！！！");
    private Dictionary<MatchController.State, Func<GameState>> states;

    public GameStateFactory()
    {   
        states = new Dictionary<MatchController.State, Func<GameState>>
        {
            { MatchController.State.GAMEOVER, () => new GameStateGameOver() },
            { MatchController.State.IN_PLAY, () => new GameStateInPlay() },
            { MatchController.State.KICKOFF, () => new GameStateKickoff() },
            { MatchController.State.OVERTIME, () => new GameStateOvertime() },
            { MatchController.State.RESET, () => new GameStateReset() },
            { MatchController.State.SCORED, () => new GameStateScored() },
        };
    }
    public GameState GetFreshState(MatchController.State state)
    {
        if (states.TryGetValue(state, out var factory)) {
            return factory();
        }
        throw new ArgumentException($"GameState not registered: {state}");
    }
}