using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateInPlay : GameState
{
    public override void OnEnter() {
        GameInterface.Interface.EventSystem.Subscribe<OnTeamScoredEvent>(OnTeamScored);
    }

    public override void OnExit() {
        GameInterface.Interface.EventSystem.Unsubscribe<OnTeamScoredEvent>(OnTeamScored);
    }

    public override void _Update() {
        float delta = Time.deltaTime;

        manager.timeLeft -= delta;

        if (manager.IsTimeUp())
        {
            if (manager.currentMatch.IsTied())
            {
                TransitionState(GameManager.State.OVERTIME);
            }
            else
            {
                TransitionState(GameManager.State.GAMEOVER);
            }
        }
    }

    private void OnTeamScored(OnTeamScoredEvent obj) {
        TransitionState(GameManager.State.SCORED,GameStateData.Build().SetCountryScoredOn(obj.CountryScoredOn));
    }
}
