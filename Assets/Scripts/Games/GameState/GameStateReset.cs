using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateReset : GameState
{
    public override void OnEnter() {

        GameInterface.Interface.EventSystem.Subscribe<OnKickoffReadyEvent>(OnKickoffReadyEvent);

    }

    public override void OnExit() {
        GameInterface.Interface.EventSystem.Unsubscribe<OnKickoffReadyEvent>(OnKickoffReadyEvent);
    }

    private void OnKickoffReadyEvent(OnKickoffReadyEvent obj) {
        if (_transitionQueued)
            return;
        _transitionQueued = true;

        Invoker.Instance.DelegateList.Add(() => {
            TransitionState(MatchController.State.KICKOFF,stateData);
        });
    }
}
