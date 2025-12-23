using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateScored : GameState
{
    private const float DURATION_CELEBRATION = 3f;
    private float timeSinceCelebration;
    public override void OnEnter() {
        manager.IncreaseScore(stateData.CountryScoredOn);
        timeSinceCelebration = Time.time;
    }

    public override void _Update() {
        if (Time.time - timeSinceCelebration > DURATION_CELEBRATION)
        {
            TransitionState(GameManager.State.RESET, stateData);
        }
    }
}
