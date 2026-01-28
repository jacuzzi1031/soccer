using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSimState 
{   
    protected PlayerSim playerSim;
    protected PlayerStateData stateData;
    public event Action<PlayerStateId, PlayerStateData> StateTransitionRequested;
    public void Setup(
        PlayerSim contextPlayerView,
        PlayerStateData contextData
    )
    {
        playerSim = contextPlayerView;
        stateData = contextData;
    }
    protected void TransitionState(PlayerStateId newState, PlayerStateData data = null)
    {
        StateTransitionRequested?.Invoke(newState, data ?? PlayerStateData.Build());
    }
    public virtual void _Update() {
        
    }
    public virtual void _FixedUpdate() {
        
    }

    public virtual void OnEnter() {
        
    }
    public virtual void OnExit() {
    }
    public virtual bool IsReadyForKickoff() => false;
}
