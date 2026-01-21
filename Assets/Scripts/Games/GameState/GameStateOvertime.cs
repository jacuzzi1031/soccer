using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateOvertime : GameState
{
    public override void OnEnter() {
        GameInterface.Interface.EventSystem.Subscribe<OnTeamScoredEvent>(onTeamScoredEvent);
        MusicManager.Instance.Play(
            MusicManager.Instance.Refs.WIN
        );
    }

    public override void OnExit() {
        GameInterface.Interface.EventSystem.Unsubscribe<OnTeamScoredEvent>(onTeamScoredEvent);
    }

    private void onTeamScoredEvent(OnTeamScoredEvent obj) {
        if (_transitionQueued)
            return;
        _transitionQueued = true;
        Invoker.Instance.DelegateList.Add(() => {
            System.IncreaseScore(obj.CountryScoredOn);
            TransitionState(MatchSystem.State.GAMEOVER);
        });
    }
}
