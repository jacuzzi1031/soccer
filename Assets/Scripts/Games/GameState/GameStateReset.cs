using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateReset : GameState
{
    public override void OnEnter() {
        GameInterface.Interface.EventSystem.Publish(new OnTeamResetEvent());
        GameInterface.Interface.EventSystem.Subscribe<OnKickoffReadyEvent>(OnKickoffReadyEvent);
    }

    public override void OnExit() {
        GameInterface.Interface.EventSystem.Unsubscribe<OnKickoffReadyEvent>(OnKickoffReadyEvent);
    }

    private void OnKickoffReadyEvent(OnKickoffReadyEvent obj) {
        TransitionState(GameManager.State.KICKOFF,stateData);
    }
}
