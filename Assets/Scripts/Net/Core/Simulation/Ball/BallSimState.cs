using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSimState
{
    public const float GRAVITY = 8f;
    protected BallSim ballSim=null;
    public PlayerView carrier=null;
    protected BallStateData stateData = new BallStateData();
    public event Action<BallStateId, BallStateData> StateTransitionRequested;
    public const float BOUNCINESS = 0.8f;
    const float idleThreshold = 0.01f;

    public virtual void Setup(BallSim contextBallSim,BallStateData ContextBallStateData){
        ballSim = contextBallSim;
        stateData = ContextBallStateData;
    }
    public void TransitionState(BallStateId newBallStateId, BallStateData data = null)
    {
        StateTransitionRequested?.Invoke(newBallStateId, data ?? BallStateData.Build());
    }
    public virtual void _Update() {
    }
    public virtual void _FixedUpdate() {
        
    }
    public virtual void OnExit() {
    }
    public virtual void OnEnter() {}
}
