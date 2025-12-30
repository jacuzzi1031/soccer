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
        MusicManager.Instance.Play(
            MusicManager.Instance.Refs.WIN
        );
    }

    public override void _Update() {
        if (Time.time - timeSinceCelebration > DURATION_CELEBRATION)
        {
            TransitionState(GameManager.State.RESET, stateData);
        }
    }
}
