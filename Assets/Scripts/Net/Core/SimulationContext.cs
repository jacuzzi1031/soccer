using System.Collections;
using System.Collections.Generic;
using Net.Core.Simulation.SimSignal;
using Net.FixFloat;
using UnityEngine;

public sealed class SimulationContext
{
    public int Frame { get; private set; }
    public static readonly FixedFloat FixedDeltaTime =
        (FixedFloat)(1f / 60f);
    public const int INVALID_PLAYER_ID = -1;
    public MatchState MatchState { get; private set; }
    public SimulationModel _simulationModel;

    public SimulationContext(SimulationModel model) {
        _simulationModel=model;
    }
    // 获取球位置和球主
    public FixedVector2 BallPosition => _simulationModel.BallSim.Position;
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

    public void Restore(FixedGameState snapshotState)
    {

        //Ball
        var ball = _simulationModel.BallSim;
        var snapshotStateBall = snapshotState.Ball;

        ball.Position = snapshotStateBall.ballPosition;
        ball.Velocity = snapshotStateBall.ballVelocity;
        ball.Height = snapshotStateBall.ballHeight;
        ball.HeightVelocity = snapshotStateBall.ballHeightVelocity;
        if (ball.BallCarrierId >= 0)
        {
            var carrier = _simulationModel.PlayerSystem.GetPlayer(ball.BallCarrierId);

            if (carrier != null)
            {
                ball.carrier = carrier;
            }
        }
        else
        {
            ball.carrier = null;
        }

        ball.restoreState( snapshotStateBall.ballState, snapshotStateBall.stateFrame );

        //Players
        foreach (var playerState in snapshotState.Players)
        {
            var player = _simulationModel.PlayerSystem.GetPlayer(playerState.playerId);

            if (player == null)
                continue;

            player.Position = playerState.playerPosition;
            player.Velocity = playerState.playerVelocity;
            player.Height = playerState.playerHeight;
            player.HeightVelocity = playerState.playerHeightVelocity;
            player.HeadingRight = playerState.HeadingRight;

            player.restoreState(
                playerState.playerState,
                playerState.stateFrame
            );
        }
        
 
    }
}
