using System;
using UnityEngine;

public class GameState
{

    public event Action<GameManager.State, GameStateData> OnStateTransitionRequested;


    protected GameManager manager;
    protected GameStateData stateData;


    public void Setup(GameManager contextManager, GameStateData contextData)
    {
        manager = contextManager;
        stateData = contextData;
    }
    
    public void TransitionState(GameManager.State newState, GameStateData data = null)
    {
        if (data == null)
            data = GameStateData.Build();

        OnStateTransitionRequested?.Invoke(newState, data);
    }
    public virtual void OnEnter() {
        
    }
    public virtual void OnExit() {
        
    }

    public virtual void _Update() {
        
    }
}