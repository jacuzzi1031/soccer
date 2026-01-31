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
    public SimEventBus _eventBus;
    public int ownerId=-1;

    public BallSim(Vector2 ContextSpawnPosition,SimEventBus eventBus)
    {
        spawnPosition = ContextSpawnPosition;
        _eventBus = eventBus;
        Position=spawnPosition;
    }
    public void SwitchState(BallStateId type, BallStateData data = null)
    {
        CurrentViewState?.OnExit();

        CurrentViewState = stateFactory.GetFreshState(type);
        CurrentViewState.Setup(this, data ?? new BallStateData());
        CurrentViewState.OnEnter();
    }

    public void Tick(ISimulationContext context) {
        
    }

    public void Stop()
    {
        if (!_running) return;
        _running = false;
    }

    public void ResetBall() {
        SwitchState(BallStateId.FREEFORM);
        _eventBus.Publish(new BallResetSignal());
    }
}
