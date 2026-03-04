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

    /*
     *Tick N：enqueue（条件检查）
    Invoker 执行：
    可能执行
    也可能没执行（留到 Tick N+1）
    与此同时：
    Tick N+1：
    _Update 又 enqueue 一条
    多条“切 State 候选命令” ，所以需要标记protected bool _transitionQueued;
     * 
     */
    public override void _Update(int frame) {
        if (_transitionQueued)
            return;
        
        Controller.timeLeft -=SimulationClock.FRAME_DT;
        
        if (!Controller.IsTimeUp())
            return;

        _transitionQueued = true;
        
        Invoker.Instance.DelegateList.Add(() => {
            if (Controller.IsTimeUp())
            {
                if (Controller.currentMatch.IsTied())
                {
                    TransitionState(MatchController.State.OVERTIME);
                }
                else
                {
                    TransitionState(MatchController.State.GAMEOVER);
                }
            }
        });
    }

    private void OnTeamScored(OnTeamScoredEvent obj) {
        Invoker.Instance.DelegateList.Add(() => {
            TransitionState(MatchController.State.SCORED, GameStateData.Build().SetCountryScoredOn(obj.CountryScoredOn));
        });
    }
}
