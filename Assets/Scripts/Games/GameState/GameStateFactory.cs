using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStateFactory
{
    private void Awake() => Debug.Log("test！！！！！");
    private Dictionary<GameManager.State, Func<GameState>> states;

    public GameStateFactory()
    {   
        states = new Dictionary<GameManager.State, Func<GameState>>
        {
            { GameManager.State.GAMEOVER, () => new GameStateGameOver() },
            { GameManager.State.IN_PLAY, () => new GameStateInPlay() },
            { GameManager.State.KICKOFF, () => new GameStateKickoff() },
            { GameManager.State.OVERTIME, () => new GameStateOvertime() },
            { GameManager.State.RESET, () => new GameStateReset() },
            { GameManager.State.SCORED, () => new GameStateScored() },
        };
    }
    public GameState GetFreshState(GameManager.State state)
    {
        if (states.TryGetValue(state, out var factory)) {
            return factory();
        }
        throw new ArgumentException($"GameState not registered: {state}");
    }
}