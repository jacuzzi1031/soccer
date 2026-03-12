using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SimulationContext
{
    public int Frame { get; private set; }
    public float DeltaTime = SimulationClock.FRAME_DT;
    public const int INVALID_PLAYER_ID = -1;
    public MatchState MatchState { get; private set; }
    public SimulationModel _simulationModel;

    public SimulationContext(SimulationModel model) {
        _simulationModel=model;
    }
    // 获取球位置和球主
    public Vector2 BallPosition => _simulationModel.BallSim.Position;
    public bool ResetBall => _simulationModel.BallSim.Position == _simulationModel.BallSim.spawnPosition;
    public int BallCarrierId => 
        _simulationModel.BallSim.carrier?.playerId ?? INVALID_PLAYER_ID;
    public bool BallCanAirInteract => _simulationModel.BallSim.CanAirInteract();

    // 获取玩家列表
    private IReadOnlyList<SimulationCommand> _commands;
    public IReadOnlyList<SimulationCommand> Commands => _commands;
    public void BuildFrom(
        int frame,
        IReadOnlyList<SimulationCommand> commands
    )
    {
        Frame = frame;
        _commands=commands;
        MatchState = _simulationModel.MatchSystem.currentState;
    }
}
