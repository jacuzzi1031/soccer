using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchSystem:ISimulationSystem
{
    public Match currentMatch;
    // private const float DURATION_GAME_SEC = 2f;
    private const float DURATION_GAME_SEC = 2 * 60f;
    
    public enum State
    {
        IN_PLAY,
        SCORED,
        RESET,
        KICKOFF,
        OVERTIME,
        GAMEOVER
    }

    [HideInInspector] public GameState currentState;
    private GameStateFactory _gameStateFactory=new GameStateFactory();
    public float timeLeft;

    public MatchSystem(string countryHome, string CountryAway) {
        currentMatch=new Match(countryHome,CountryAway);
        StartGame();
    }

    public void StartGame()
    {
        timeLeft = DURATION_GAME_SEC;
        SwitchGameState(State.RESET);
    }

    public void SwitchGameState(State newState, GameStateData data = null) {
        if (currentState != null) {
            currentState.OnStateTransitionRequested-= SwitchGameState;
            currentState.OnExit();
        }
        currentState = _gameStateFactory.GetFreshState(newState);
        currentState.Setup(this,data);
        currentState.OnStateTransitionRequested+= SwitchGameState;
        currentState.OnEnter();
    }
    public bool IsTimeUp()
    {
        return timeLeft <= 0f;
    }
    public string GetWinnerCountry()
    {
        if (currentMatch.IsTied())
            throw new InvalidOperationException("Cannot get winner: match is tied.");

        return currentMatch.winner;
    }
    public void IncreaseScore(string countryScoredOn)
    {
        currentMatch.IncreaseScore(countryScoredOn);
        GameInterface.Interface.EventSystem.Publish(new OnScoreChangedEvent());
    }
    public void Tick(int frame) {
        currentState?._Update(frame);
    }

    public void Stop() {
        currentState.OnStateTransitionRequested-= SwitchGameState;
        currentMatch = null;
    }

    public void AllReady() {
        Invoker.Instance.DelegateList.Add(() => {
            
            GameInterface.Interface.EventSystem.Publish(new OnKickoffReadyEvent());
        });
    }
}
