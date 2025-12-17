using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
    public enum MatchType {
    Training,          //训练模式
    TrainingWithEnemy,//对抗训练
    UltimateTeam     //ut
    }
    public MatchType currentMatchType;

    public enum GameMode {
        Single,
        Versus,
        Coop
    }
    public GameMode currentGameMode;

    public Match currentMatch;
    public string[] playerSetup = { "FRANCE", "" };
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
    public float TimeLeft { get; private set; }
    public void OnInit()
    {
        currentMatch = new Match("ARGENTINA", "SPAIN");
        playerSetup[0] = currentMatch.countryHome;

    }
    public void StartGame()
    {
        TimeLeft = DURATION_GAME_SEC;
        SwitchGameState(State.RESET);
    }

    public void SwitchGameState(State newState, GameStateData data = null) {
        if (currentState != null) {
            currentState.OnStateTransitionRequested+= SwitchGameState;
            currentState.OnExit();
        }
        currentState = _gameStateFactory.GetFreshState(newState);
        currentState.Setup(this,data);
        currentState.OnStateTransitionRequested+= SwitchGameState;
        currentState.OnEnter();
    }
    public bool IsTimeUp()
    {
        return TimeLeft <= 0f;
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
    public void SetCurrentMatchType(MatchType matchType) {
        currentMatchType=matchType;
    }

    public void SetMatchCountry(int RoomIndex, string Country) {
        playerSetup[RoomIndex] = Country;
        //锦标赛还会改
        if (RoomIndex == 0) {
            currentMatch.countryHome=playerSetup[RoomIndex];
        }
        else {
            currentMatch.countryAway=playerSetup[RoomIndex];
        }
    }

    public void SetGameMode(GameMode gameMode) {
        currentGameMode = gameMode;
    }
}
