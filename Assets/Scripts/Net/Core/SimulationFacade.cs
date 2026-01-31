using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationFacade
{
    public PlayerSystem _playerSystem { get; }
    public BallSim _ballSim { get; }
    public SimulationFacade(
        PlayerSystem playerSystem,
        BallSim ballSim)
    {
        _playerSystem = playerSystem;
        _ballSim = ballSim;
    }
    public void ResetTeams()
    {
        _playerSystem.ResetTeams();
        _ballSim.ResetBall();
    }
}
