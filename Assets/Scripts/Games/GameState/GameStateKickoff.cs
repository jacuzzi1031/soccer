using System.Collections;
using System.Collections.Generic;
using SocketProtocol;
using UnityEngine;

public class GameStateKickoff : GameState {
    private HashSet<int> validEntityIdSet=new HashSet<int>();

    public override void OnEnter() {
        validEntityIdSet.Clear();
        string countryStarting = stateData.CountryScoredOn;
        if (countryStarting == null) countryStarting = Controller.currentMatch.countryHome;
        bool ScoreIsHome=Controller.currentMatch.countryHome==countryStarting?true:false;

        
        RoomMatchType matchType = GameInterface.Interface.GameManager.currentMatchType;
        GameManager.GameMode gameMode = GameInterface.Interface.GameManager.currentGameMode;
        if (matchType == RoomMatchType.Training || matchType == RoomMatchType.TrainingWithEnemy
                                                ||gameMode==GameManager.GameMode.Single) {
        }
        
        GameInterface.Interface.EventSystem.Subscribe<EntityForGameKickoffEvent>(onKickoffEvent);
        SoundManager.Instance.Play(SoundManager.Instance.audioRefs.WHISTLE);
    }

    public override void OnExit() {
        GameInterface.Interface.EventSystem.Unsubscribe<EntityForGameKickoffEvent>(onKickoffEvent);
    }

    private void onKickoffEvent(EntityForGameKickoffEvent obj) {
        
        if (!validEntityIdSet.Contains(obj.entityId))
            return;
        if (_transitionQueued)
            return;
        _transitionQueued = true;

        Invoker.Instance.DelegateList.Add(() => {
            TransitionState(MatchController.State.IN_PLAY);
            GameInterface.Interface.EventSystem.Publish(new OnKickoffStartedEvent());
        });
    }
}
