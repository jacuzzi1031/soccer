using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchController : MonoBehaviour
{
    public Match currentMatch;
    // private const float DURATION_GAME_SEC = 2f;
    private const float DURATION_GAME_SEC = 2 * 60f;
    private SimEventBus _eventBus;
    private SimulationFacade _simulation;
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

    public MatchController(SimulationFacade simulation,SimEventBus EventBus,string countryHome, string CountryAway) {
        currentMatch=new Match(countryHome,CountryAway);
        _eventBus = EventBus;
        _simulation = simulation;
        SubscribeSimSignal();
        
        StartGame();
    }

    private void SubscribeSimSignal() {
        _eventBus.Subscribe<ControllerChangedSignal>(OnControllerChanged);
        _eventBus.Subscribe<BallResetSignal>(OnBallReset);
    }

    private void OnBallReset(BallResetSignal obj) {
        Invoker.Instance.DelegateList.Add(() =>
        {
            GameInterface.Interface.EventSystem.Publish(
                new BallResetEvent()
            );
        });
    }

    void OnControllerChanged(ControllerChangedSignal s)
    {
        //不只是顺序，而是「确定性边界 + 执行域隔离」
        Invoker.Instance.DelegateList.Add(() =>
        {
            GameInterface.Interface.EventSystem.Publish(
                new SetControllerEvent(s.HomePlayerId, s.AwayPlayerId)
            );
        });
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

    public void RequestReset() {
        _simulation.ResetTeams();
    }
}
