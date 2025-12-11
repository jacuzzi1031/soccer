using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateReset : GameState
{
    public override void OnEnter() {
        GameInterface.Interface.EventSystem.Publish(new TeamResetEvent());
        GameInterface.Interface.EventSystem.Subscribe<OnKickoffReadyEvent>(OnKickoffReadyEvent);
    }

    private void OnKickoffReadyEvent(OnKickoffReadyEvent obj) {
        TransitionState(GameManager.State.KICKOFF,stateData);
    }
}
