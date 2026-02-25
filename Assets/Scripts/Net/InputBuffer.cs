using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameFrameSync;
using Unity.VisualScripting;
using UnityEngine;

public class InputBuffer
{
    public class Command
    {
        public int frame;
        public int playerId;
        public int slot;
        public int inputType;
        public Vector2 moveVector;
        public int ownerId;
    }
// frame -> [player0Cmd, player1Cmd]
    private readonly Dictionary<int, Command[]> buffer;

    private readonly ObjectPool<Command> _commandPool;
    public  int maxPlayers;
    private Dictionary<int, int> _playerIdToSlot;
    public InputBuffer() {
        buffer = new Dictionary<int, Command[]>();
        _commandPool = new ObjectPool<Command>(() => new Command());
    }
    public void Push(ResFrameInputData msg)
    {
        if (!_playerIdToSlot.TryGetValue(msg.OwnerPlayerId, out var slot))
        {
            // 非法玩家，丢弃
            return;
        }

        if ((uint)slot >= (uint)maxPlayers)
            return;

        if (!buffer.TryGetValue(msg.FrameId, out var frameCmds))
        {
            frameCmds = new Command[maxPlayers];
            buffer.Add(msg.FrameId, frameCmds);
        }

        if (frameCmds[slot] != null)
        {
            _commandPool.Release(frameCmds[slot]);
        }

        var cmd = _commandPool.Allocate();
        cmd.frame = msg.FrameId;
        cmd.slot = slot;          
        cmd.ownerId = msg.OwnerPlayerId; 
        cmd.inputType = msg.InputType;
        cmd.moveVector = DecodeMoveDir(msg.MoveVector);

        frameCmds[slot] = cmd;
    }
    public static Vector2 DecodeMoveDir(Vector2D v)
    {
        if (v.X == 0 && v.Y == 0)
            return Vector2.zero;

        return new Vector2(v.X, v.Y).normalized;
    }

    public void ConsumeFrame(int frame, Action<Command> visitor)
    {
        if (!buffer.TryGetValue(frame, out var frameCmds))
            return;

        for (int slot = 0; slot < frameCmds.Length; slot++)
        {
            var cmd = frameCmds[slot];

            if (cmd == null)
            {
                // 这里说明你的窗口判断有 bug
                throw new InvalidOperationException(
                    $"Frame {frame} slot {slot} missing input.");
            }

            visitor(cmd);
            _commandPool.Release(cmd);
            frameCmds[slot] = null;
        }

        buffer.Remove(frame);
    }


    public void SetPlayerIdToSlot(ControlContext controlContext) {
        _playerIdToSlot=new Dictionary<int, int>();
        int slotId = 0;
        if (controlContext.HomeOwnerId != -1) {
            _playerIdToSlot[controlContext.HomeOwnerId] = slotId++;
        }
        if (controlContext.AwayOwnerId != -1) {
            _playerIdToSlot[controlContext.AwayOwnerId] = slotId++;
        }

        maxPlayers = slotId;
    }
}

