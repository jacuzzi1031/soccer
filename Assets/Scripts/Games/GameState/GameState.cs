using System;
using UnityEngine;

public class GameState
{
    public event Action<MatchController.State, GameStateData> OnStateTransitionRequested;


    protected MatchController Controller;
    protected GameStateData stateData;
    protected bool _transitionQueued;


    public void Setup(MatchController contextController, GameStateData contextData)
    {
        Controller = contextController;
        stateData = contextData;
    }
    
    public void TransitionState(MatchController.State newState, GameStateData data = null)
    {
        if (data == null)
            data = GameStateData.Build();

        OnStateTransitionRequested?.Invoke(newState, data);
    }
    public virtual void OnEnter() {
        
    }
    public virtual void OnExit() {
        
    }

    public virtual void _Update(int frame) {
        
    }
}