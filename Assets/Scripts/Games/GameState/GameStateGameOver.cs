using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateGameOver : GameState
{
    public override void OnEnter() {
        string winnerCountry = manager.GetWinnerCountry();
        GameInterface.Interface.EventSystem.Publish(new OnGameOverEvent(winnerCountry));
        SoundManager.Instance.Play(SoundManager.Instance.audioRefs.WHISTLE);
    }


}
