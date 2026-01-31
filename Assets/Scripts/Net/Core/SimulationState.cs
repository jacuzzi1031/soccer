using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SimulationState
{
    public BallSim Ball;
    public List<PlayerSim> Players = new();
}

