using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSystem:ISimulationSystem
{
    private bool _running;

    public BallSystem()
    {
    }

    public void Start()
    {
        if (_running) return;
        _running = true;
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
