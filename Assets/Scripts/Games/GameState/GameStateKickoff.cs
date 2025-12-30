using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateKickoff : GameState {
    private HashSet<int> validEntityIdSet=new HashSet<int>();

    public override void OnEnter() {
        validEntityIdSet.Clear();
        string countryStarting = stateData.CountryScoredOn;
        if (countryStarting == null) countryStarting = manager.playerSetup[0];
        bool ScoreIsHome=manager.playerSetup[0]==countryStarting?true:false;
        List<int> entityIdListIsHome = EntityManager.Instance.GetEntityIdListIsHome(ScoreIsHome);
        
        validEntityIdSet = new HashSet<int>(entityIdListIsHome);
        
        GameManager.MatchType matchType = GameInterface.Interface.GameManager.currentMatchType;
        GameManager.GameMode gameMode = GameInterface.Interface.GameManager.currentGameMode;
        if (matchType == GameManager.MatchType.Training || matchType == GameManager.MatchType.TrainingWithEnemy
            ||gameMode==GameManager.GameMode.Single) {
            validEntityIdSet.Add(EntityManager.Instance.GetEntityByID(0));
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
        GameInterface.Interface.EventSystem.Publish(new OnKickoffStartedEvent());
        TransitionState(GameManager.State.IN_PLAY);
    }
}
