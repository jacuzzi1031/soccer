using System;
using System.Collections;
using System.Collections.Generic;
using Net.Core.Simulation.SimSignal;
using UnityEngine;

public class MatchController
{
    // private const float DURATION_GAME_SEC = 2f;
    private const float DURATION_GAME_SEC = 2 * 60f;
    private SimEventBus _eventBus;
    public string countryHome;
    public string countryAway;
    private MatchSystem _matchSystem;
    public int FramesLeft => _matchSystem.framesLeft;
    public int GoalsHome => _matchSystem.goalsHome;
    public int GoalsAway => _matchSystem.goalsAway;
    public string Winner=>_matchSystem.goalsHome>_matchSystem.goalsAway?countryHome:countryAway;
    public enum State
    {
        IN_PLAY,
        SCORED,
        RESET,
        KICKOFF,
        OVERTIME,
        GAMEOVER
    }
    public MatchController(MatchSystem matchSystem,SimEventBus EventBus,string CountryHome, string CountryAway) {
        countryHome= CountryHome;
        countryAway= CountryAway;
        _matchSystem=matchSystem;
        _eventBus = EventBus;
        SubscribeSimSignal();
        GameInterface.Interface.EventSystem.Publish(new MatchStartEvent());

    }

    private void SubscribeSimSignal() {
        _eventBus.Subscribe<TeamResetSignal>(OnTeamReset);
        _eventBus.Subscribe<ControllerChangedSignal>(OnControllerChanged);
        _eventBus.Subscribe<KickoffStartSignal>(OnKickoffStart);
        _eventBus.Subscribe<PlayStyleShowSignal>(OnPlayStyleShow);
        _eventBus.Subscribe<TeamScoringSignal>(OnTeamScoring);
        _eventBus.Subscribe<BallBacktoSpawnPositionSignal>(OnballBacktoSpawnPosition);
        _eventBus.Subscribe<GameOverSignal>(OnGameOver);
        _eventBus.Subscribe<OnScoreChangedSignal>(OnScoreChanged);
        _eventBus.Subscribe<goalKeeperBounceBallSignal>(OnGoalKeeperBounceBall);
        _eventBus.Subscribe<PlayVolleyShotSignal>(OnPlayVolleyShot);
    }

    private void OnPlayVolleyShot(PlayVolleyShotSignal obj) {

            GameInterface.Interface.EventSystem.Publish(new PlayerVolleyShotEvent(obj.PlayerName));

    }

    private void OnGoalKeeperBounceBall(goalKeeperBounceBallSignal obj) {

            SoundManager.Instance.Play(SoundManager.Instance.audioRefs.BOUNCE);

    }

    private void OnScoreChanged(OnScoreChangedSignal obj) {

            GameInterface.Interface.EventSystem.Publish(new OnScoreChangedEvent());

    }

    public bool HasSomeoneScored()
    {
        return GoalsHome > 0 || GoalsAway > 0;
    }
    private void OnGameOver(GameOverSignal obj) {

            GameInterface.Interface.EventSystem.Publish(new OnGameOverEvent());

    }

    private void OnballBacktoSpawnPosition(BallBacktoSpawnPositionSignal obj) {

            GameInterface.Interface.EventSystem.Publish(new BallBacktoSpawnPositionEvent());

    }

    private void OnTeamScoring(TeamScoringSignal obj) {

            SoundManager.Instance.Play(SoundManager.Instance.audioRefs.WHISTLE);

    }

    private void OnPlayStyleShow(PlayStyleShowSignal obj) {

            GameInterface.Interface.EventSystem.Publish(new PlayStyleShowEvent(obj.playerId,obj.playerState));

    }

    private void OnKickoffStart(KickoffStartSignal obj) {

            SoundManager.Instance.Play(SoundManager.Instance.audioRefs.WHISTLE);

    }

    private void OnTeamReset(TeamResetSignal obj) {

            GameInterface.Interface.EventSystem.Publish(new OnTeamResetEvent());
            MusicManager.Instance.Play(
                MusicManager.Instance.Refs.GAMEPLAY
            );
    }
    

    void OnControllerChanged(ControllerChangedSignal s)
    {
        //不只是顺序，而是「确定性边界 + 执行域隔离」

            GameInterface.Interface.EventSystem.Publish(
                new OnControlSwitchEvent(s.OldPlayerId, s.NewPlayerId,s.Scheme)
            );

    }
    


    public bool IsTimeUp()
    {
        return FramesLeft <= 0;
    }
}
