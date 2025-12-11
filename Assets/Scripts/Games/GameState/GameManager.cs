using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get;private set; }

    public enum MatchType {
    Training,
    TrainingWithEnemy,
    Championship
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
    private GameStateFactory gameStateFactory=new GameStateFactory();
    public float TimeLeft { get; private set; }
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
        currentState = gameStateFactory.GetFreshState(newState);
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
}
