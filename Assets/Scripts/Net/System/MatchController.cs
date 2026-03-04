using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchController
{
    public Match currentMatch;
    // private const float DURATION_GAME_SEC = 2f;
    private const float DURATION_GAME_SEC = 2 * 60f;
    private SimEventBus _eventBus;
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
    public MatchController(SimEventBus EventBus,string countryHome, string CountryAway) {
        currentMatch=new Match(countryHome,CountryAway);
        _eventBus = EventBus;
        SubscribeSimSignal();
        
                
        Invoker.Instance.DelegateList.Add(() => {
            GameInterface.Interface.EventSystem.Publish(new MatchStartEvent());
        });
    }

    private void SubscribeSimSignal() {
        _eventBus.Subscribe<TeamResetSignal>(OnTeamReset);
        _eventBus.Subscribe<ControllerChangedSignal>(OnControllerChanged);
        _eventBus.Subscribe<KickoffStartSignal>(OnKickoffStart);
        _eventBus.Subscribe<PlayStyleShowSignal>(OnPlayStyleShow);
    }

    private void OnPlayStyleShow(PlayStyleShowSignal obj) {
        Invoker.Instance.DelegateList.Add(() => {
            GameInterface.Interface.EventSystem.Publish(new PlayStyleShowEvent(obj.playerId,obj.playerState));
        });
    }

    private void OnKickoffStart(KickoffStartSignal obj) {
        Invoker.Instance.DelegateList.Add(() => {
            SoundManager.Instance.Play(SoundManager.Instance.audioRefs.WHISTLE);
        });
    }

    private void OnTeamReset(TeamResetSignal obj) {
        Invoker.Instance.DelegateList.Add(() =>
        {
            GameInterface.Interface.EventSystem.Publish(new OnTeamResetEvent());
            MusicManager.Instance.Play(
                MusicManager.Instance.Refs.GAMEPLAY
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
}
