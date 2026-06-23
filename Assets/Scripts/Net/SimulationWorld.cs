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
    private Dictionary<int, SnapshotData> _snapshots = new();
    public const int SNAPSHOT_INTERVAL = 30;
    public const int CHECKSUM_INTERVAL = 120;

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
    public void Step(
        int frame,
        bool isRollback = false)
    {
        var cmds = InputBuffer.GetFrameCommands(frame);
        
        ExecuteFrame(frame, cmds);

        if (!isRollback)
        {
            if(frame % SNAPSHOT_INTERVAL == 0)
                SaveSnapshot(frame);

            if(frame % CHECKSUM_INTERVAL == 0)
                SendChecksum(frame);
        }
    }
    private void ExecuteFrame(
        int frame,
        InputBuffer.Command[] cmds) {
        
        if (cmds != null)
        {
            foreach (var cmd in cmds)
            {
                if(cmd != null)
                    DispatchCommand(cmd);
            }
        }
        context.BuildFrom(frame, commandBuffer.Consume());

        for (int i = 0; i < systems.Count; i++)
        {
            systems[i].Tick(context);
        }
        eventBus.Flush();
    }
    private void SaveSnapshot(int frame)
    {
        var fixedGameState = ExtractGameState(frame);
        _snapshots[frame] =  new SnapshotData
        {
            Frame = frame,
            State = fixedGameState
        };

        int expireFrame = frame - SNAPSHOT_INTERVAL * 20;
        _snapshots.Remove(expireFrame);
    }
    private FixedGameState ExtractGameState(int frame)
    {
        var models = context._simulationModel;

        var snapshot = new FixedGameState
        {
            Frame = frame,
            Ball = new FixedBallState
            {
                ballPosition = models.BallSim.Position,
                ballVelocity = models.BallSim.Velocity,
                ballHeight = models.BallSim.Height,
                ballHeightVelocity = models.BallSim.HeightVelocity,
                ballState = models.BallSim.ballState,
                ballCarrierId = models.BallSim.BallCarrierId,
                stateFrame=models.BallSim.currentState.stateFrame,
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
                HeadingRight = player.HeadingRight,
                stateFrame=player.currentState.stateFrame
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
                HeadingRight = player.HeadingRight,
                stateFrame=player.currentState.stateFrame,
            });
        }
        return snapshot;
    }
    private ObjectPool<ReqChecksum> _checksumPool =
        new ObjectPool<ReqChecksum>(() => new ReqChecksum());

    private ObjectPool<ReqFrameSyncData> _syncDataPool =
        new ObjectPool<ReqFrameSyncData>(() => new ReqFrameSyncData());
    private void SendChecksum(int frame)
    {
        var req = _syncDataPool.Allocate();

        req.MessageType = MessageType.Checksum;

        var checksum = _checksumPool.Allocate();

        checksum.FrameId = frame;
        checksum.SeatIndex=GameInterface.Interface.RoomManager.localSeatIndex;
        checksum.ChecksumValue = ComputeChecksum();

        req.Checksum = checksum;
        req.RoomCode = GameInterface.Interface.RoomManager.CurrentRoomInfo.roomCode;
        GameInterface.Interface.UdpListener.Send(req);

        _checksumPool.Release(checksum);
        _syncDataPool.Release(req);
    }
    private ulong ComputeChecksum()
    {
        const ulong Offset = 14695981039346656037UL;
        const ulong Prime = 1099511628211UL;

        ulong hash = Offset;

        void Add(ulong value)
        {
            hash ^= value;
            hash *= Prime;
        }

        var models = context._simulationModel;

        Add((ulong)context.Frame);

        Add((ulong)models.BallSim.Position.x.ScaledValue);
        Add((ulong)models.BallSim.Position.y.ScaledValue);
        Add((ulong)models.BallSim.HeightVelocity.ScaledValue);
        Add((ulong)models.BallSim.Height.ScaledValue);
        Add((ulong)(int)models.BallSim.ballState);

        foreach (var player in models.PlayerSystem.teamHome)
        {
            Add((ulong)player.playerId);
            Add((ulong)player.Position.x.ScaledValue);
            Add((ulong)player.Position.y.ScaledValue);
            Add((ulong)player.HeightVelocity.ScaledValue);
            Add((ulong)player.Height.ScaledValue);
            Add((ulong)(int)player.playerState);
            Add(player.HeadingRight ? 1UL : 0UL);
        }

        foreach (var player in models.PlayerSystem.teamAway)
        {
            Add((ulong)player.playerId);
            Add((ulong)player.Position.x.ScaledValue);
            Add((ulong)player.Position.y.ScaledValue);
            Add((ulong)player.HeightVelocity.ScaledValue);
            Add((ulong)player.Height.ScaledValue);
            Add((ulong)(int)player.playerState);
        }

        return hash;
    }
    private int FindNearestSnapshot(int frame)
    {
        int snapshotFrame =
            frame - (frame % SNAPSHOT_INTERVAL);

        while (snapshotFrame >= 0)
        {
            if (_snapshots.ContainsKey(snapshotFrame))
                return snapshotFrame;

            snapshotFrame -= SNAPSHOT_INTERVAL;
        }
        return -1;
    }
    public int LoadNearestSnapshot(int desyncFrame)
    {
        int snapshotFrame = FindNearestSnapshot(desyncFrame);

        if (snapshotFrame < 0)
        {
            Debug.LogError(
                $"Rollback failed. No snapshot found. desyncFrame={desyncFrame}");
            return -1;
        }

        if (!_snapshots.TryGetValue(snapshotFrame, out var snapshot))
        {
            Debug.LogError(
                $"Rollback failed. Snapshot missing. frame={snapshotFrame}");
            return -1;
        }

        context.Restore(snapshot.State);

        Debug.Log(
            $"Rollback restore snapshot. desync={desyncFrame}, snapshot={snapshotFrame}");

        return snapshotFrame;
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
