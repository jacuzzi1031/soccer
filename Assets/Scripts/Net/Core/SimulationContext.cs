using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SimulationContext
{
    public int Frame { get; private set; }
    public float DeltaTime { get; private set; }
    public MatchState MatchState { get; private set; }
    public SimulationModel _simulationModel;

    public SimulationContext(SimulationModel model) {
        _simulationModel=model;
    }
    // 获取球位置和球主
    public Vector2 BallPosition => _simulationModel.BallSim.Position;
    public int BallCarrierId => _simulationModel.BallSim.carrierId;
    public bool BallCanAirInteract => _simulationModel.BallSim.CanAirInteract();

    // 获取玩家列表
    private IReadOnlyList<SimulationCommand> _commands;
    public IReadOnlyList<SimulationCommand> Commands => _commands;
    public void BuildFrom(
        int frame,
        float dt,
        IReadOnlyList<SimulationCommand> commands
    )
    {
        
        Frame = frame;
        DeltaTime = dt;
        _commands=commands;
        MatchState = _simulationModel.MatchSystem.currentState;
    }
}
