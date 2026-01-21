using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateKickoff : GameState {
    private HashSet<int> validEntityIdSet=new HashSet<int>();

    public override void OnEnter() {
        validEntityIdSet.Clear();
        string countryStarting = stateData.CountryScoredOn;
        if (countryStarting == null) countryStarting = System.currentMatch.countryHome;
        bool ScoreIsHome=System.currentMatch.countryHome==countryStarting?true:false;
        List<int> entityIdListIsHome = EntityManager.Instance.GetEntityIdListIsHome(ScoreIsHome);
        
        validEntityIdSet = new HashSet<int>(entityIdListIsHome);
        
        GameManager.MatchType matchType = GameInterface.Interface.GameManager.currentMatchType;
        GameManager.GameMode gameMode = GameInterface.Interface.GameManager.currentGameMode;
        if (matchType == GameManager.MatchType.Training || matchType == GameManager.MatchType.TrainingWithEnemy
                                                        ||gameMode==GameManager.GameMode.Single) {
            validEntityIdSet.Add(EntityManager.Instance.GetEntityByID());
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
            TransitionState(MatchSystem.State.IN_PLAY);
            GameInterface.Interface.EventSystem.Publish(new OnKickoffStartedEvent());
        });
    }
}
