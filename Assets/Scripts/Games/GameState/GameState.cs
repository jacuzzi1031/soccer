using System;
using UnityEngine;

public class GameState
{
    public event Action<MatchSystem.State, GameStateData> OnStateTransitionRequested;


    protected MatchSystem System;
    protected GameStateData stateData;
    protected bool _transitionQueued;


    public void Setup(MatchSystem contextSystem, GameStateData contextData)
    {
        System = contextSystem;
        stateData = contextData;
    }
    
    public void TransitionState(MatchSystem.State newState, GameStateData data = null)
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