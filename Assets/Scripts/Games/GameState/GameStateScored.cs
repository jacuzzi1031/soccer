using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateScored : GameState
{
    private const float CELEBRATION_DURATION = 3f;
    private static readonly int CELEBRATION_DURATION_FRAMES =
        Mathf.CeilToInt(
            CELEBRATION_DURATION / SimulationClock.FRAME_DT
        );
    private int celebrationFrames;
    public override void OnEnter() {
        Controller.IncreaseScore(stateData.CountryScoredOn);
        celebrationFrames = 0;
        MusicManager.Instance.Play(
            MusicManager.Instance.Refs.WIN
        );
    }

    public override void _Update(int frame)
    {
        if (_transitionQueued)
            return;

        celebrationFrames++;

        if (celebrationFrames < CELEBRATION_DURATION_FRAMES)
            return;

        _transitionQueued = true;

        Invoker.Instance.DelegateList.Add(() =>
        {
            TransitionState(MatchController.State.RESET, stateData);
        });
    }
}
