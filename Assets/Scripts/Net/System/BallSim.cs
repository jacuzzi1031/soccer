using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSim:ISimulationSystem
{
    public Vector2 Position;
    public Vector2 spawnPosition;
    public BallSimState CurrentViewState;
    public BallSimStateFactory stateFactory;
    private bool _running;
    

    public BallSim(Vector2 ContextSpawnPosition)
    {
        spawnPosition = ContextSpawnPosition;
    }

    public void Start()
    {
        if (_running) return;
        _running = true;
        Position=spawnPosition;
    }
    public void SwitchState(BallStateId type, BallStateData data = null)
    {
        CurrentViewState?.OnExit();

        CurrentViewState = stateFactory.GetFreshState(type);
        CurrentViewState.Setup(this, data ?? new BallStateData());
        CurrentViewState.OnEnter();
    }

    public void Stop()
    {
        if (!_running) return;
        _running = false;
    }
    

    public void Tick(int frame)
    {
        if (!_running) return;
        
    }
}
