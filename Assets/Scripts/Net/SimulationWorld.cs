using System.Collections;
using System.Collections.Generic;
using System.IO;
using GameFrameSync;
using Net.Core.Simulation;
using Net.Core.Simulation.SimSignal;
using UnityEngine;

public class SimulationWorld
{
    private List<ISimulationSystem> systems;
    private SimulationContext context;
    private CommandBuffer commandBuffer;
    private SimEventBus eventBus;
    public InputBuffer InputBuffer;
    private Dictionary<int, FixedGameState> _snapshots = new();
    public const int SNAPSHOT_INTERVAL = 300;
    public SimulationWorld(
        List<ISimulationSystem> systems,
        SimulationContext context,
        CommandBuffer commandBuffer,
        SimEventBus eventBus,
        InputBuffer inputBuffer)
    {
        this.systems = systems;
        this.context = context;
        this.commandBuffer = commandBuffer;
        this.eventBus = eventBus;
        this.InputBuffer = inputBuffer;
    }

    public void Step(int frame)
    {
        InputBuffer.ConsumeFrame(frame, DispatchCommand);

        context.BuildFrom(frame,commandBuffer.Consume());

        for (int i = 0; i < systems.Count; i++)
        {
            systems[i].Tick(context);
        }
        
        eventBus.Flush();
        
        //for checksum and rollback
        if (frame % SNAPSHOT_INTERVAL == 0)
        {
            SaveSnapshot(frame);
        }

        if (frame % 120 == 0)
        {
            SendChecksum(frame);
        }
        
    }
    private void SaveSnapshot(int frame)
    {
        _snapshots[frame] = ExtractGameState();

        int expireFrame = frame - SNAPSHOT_INTERVAL * 10;
        _snapshots.Remove(expireFrame);
    }
    private FixedGameState ExtractGameState()
    {
        var models = context._simulationModel;

        var snapshot = new FixedGameState
        {
            Frame = context.Frame,
            Ball = new FixedBallState
            {
                ballPosition = models.BallSim.Position,
                ballVelocity = models.BallSim.Velocity,
                ballHeight = models.BallSim.Height,
                ballHeightVelocity = models.BallSim.HeightVelocity,
                ballState = models.BallSim.ballState,
                ballCarrierId = models.BallSim.BallCarrierId
            },
            Players = new List<FixedPlayerState>()
        };

        foreach (var player in models.PlayerSystem.teamHome)
        {
            snapshot.Players.Add(new FixedPlayerState
            {
                playerId =  player.playerId,
                playerPosition = player.Position,
                playerVelocity = player.Velocity,
                playerHeight = player.Height,
                playerHeightVelocity = player.HeightVelocity,
                playerState = player.playerState,
                HeadingRight = player.HeadingRight
            });
        }

        foreach (var player in models.PlayerSystem.teamAway)
        {
            snapshot.Players.Add(new FixedPlayerState
            {
                playerId =  player.playerId,
                playerPosition = player.Position,
                playerVelocity = player.Velocity,
                playerHeight = player.Height,
                playerHeightVelocity = player.HeightVelocity,
                playerState = player.playerState,
                HeadingRight = player.HeadingRight
            });
        }
        return snapshot;
    }
    private void SendChecksum(int frame)
    {
        ReqFrameSyncData req =
            new ReqFrameSyncData();

        req.MessageType =MessageType.Checksum;

        req.Checksum =
            new ReqChecksum();

        req.Checksum.FrameId = frame;

        req.Checksum.ChecksumValue =
            ComputeChecksum();

        GameInterface.Interface
            .UdpListener
            .Send(req);
    }
    private ulong ComputeChecksum()
    {
        byte[] data =
            SerializeDeterministicState();

        // return XXHash64.Hash(data);
        return new ulong();
    }
    public byte[] SerializeDeterministicState()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        var models = context._simulationModel;
        FixedGameState snapshot=new FixedGameState();
        bw.Write(context.Frame);

        bw.Write(models.BallSim.Position.x.ScaledValue);
        bw.Write(models.BallSim.Position.y.ScaledValue);
        bw.Write(models.BallSim.HeightVelocity.ScaledValue);
        bw.Write(models.BallSim.Height.ScaledValue);
        bw.Write((int)models.BallSim.ballState);
        foreach (var player in models.PlayerSystem.teamHome) {
            bw.Write(player.playerId);
            bw.Write(player.Position.x.ScaledValue);
            bw.Write(player.Position.y.ScaledValue);
            bw.Write(player.HeightVelocity.ScaledValue);
            bw.Write(player.Height.ScaledValue);
            bw.Write((int)player.playerState);
            bw.Write(player.HeadingRight);
        }
        foreach (var player in models.PlayerSystem.teamAway) {
            bw.Write(player.playerId);
            bw.Write(player.Position.x.ScaledValue);
            bw.Write(player.Position.y.ScaledValue);
            bw.Write(player.HeightVelocity.ScaledValue);
            bw.Write(player.Height.ScaledValue);
            bw.Write((int)player.playerState);
        }
        return ms.ToArray();
    }

    private void DispatchCommand(InputBuffer.Command cmd)
    {
        var flags = (GameInput.InputEventFlags)cmd.inputType;
        if ((flags & GameInput.InputEventFlags.Swap) != 0) {
            EnqueueCommad(SimulationCommandType.Swap, cmd);
            return;
        }
        if ((flags & GameInput.InputEventFlags.ShootRelease) != 0) {
            EnqueueCommad(SimulationCommandType.ShootRelease, cmd);
            return;
        }
        if ((flags & GameInput.InputEventFlags.ShootPress) != 0) {
            EnqueueCommad(SimulationCommandType.ShootPress, cmd);
            return;
        }
        if ((flags & GameInput.InputEventFlags.ShortPass) != 0) {
            EnqueueCommad(SimulationCommandType.ShortPass, cmd);
            return;
        }
        if ((flags & GameInput.InputEventFlags.IncisivePass) != 0) {
            EnqueueCommad(SimulationCommandType.IncisivePass, cmd);
            return;
        }
        if ((flags & GameInput.InputEventFlags.LongPass) != 0) {
            EnqueueCommad(SimulationCommandType.LongPass, cmd);
            return;
        }
        if (flags == GameInput.InputEventFlags.None) {
            EnqueueCommad(SimulationCommandType.NoneInputCommand, cmd);
            return;
        }
    }
    private void EnqueueCommad(SimulationCommandType type, InputBuffer.Command cmd)
    {
        commandBuffer.Enqueue(new SimulationCommand {
            Type = type,
            moveVector = cmd.moveVector,
            SeatIndex = cmd.seatIndex,
        });
    }
}
