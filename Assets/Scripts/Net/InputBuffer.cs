using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameFrameSync;
using UnityEngine;

public class InputBuffer
{
    public class Command
    {
        public int frame;
        public int playerId;
        public InputType inputType;
        public Vector2D dir;
        public int activeUnitIndex;
        public int commandIndex;
    }
// frame -> [player0Cmd, player1Cmd]
    private readonly Dictionary<int, Command[]> buffer;

    private readonly ObjectPool<Command> _commandPool;
    public InputBuffer() {
        buffer = new Dictionary<int, Command[]>();
        _commandPool = new ObjectPool<Command>(() => new Command());
    }
    public void Push(ResFrameInputData msg)
    {
        if (!buffer.TryGetValue(msg.FrameId, out var frameCmds))
        {
            frameCmds = new Command[2];
            buffer.Add(msg.FrameId, frameCmds);
        }

        int pid = msg.PlayerId;

        // 覆盖语义：一帧只保留最终输入
        if (frameCmds[pid] != null)
        {
            _commandPool.Release(frameCmds[pid]);
        }

        var cmd = _commandPool.Allocate();
        cmd.frame = msg.FrameId;
        cmd.playerId = pid;
        cmd.inputType = msg.InputType;
        cmd.dir = msg.MoveVector;
        cmd.activeUnitIndex = msg.ActiveUnitIndex;
        cmd.commandIndex = msg.CommandIndex;

        frameCmds[pid] = cmd;
    }


    public void ConsumeFrame(int frame, Action<Command> visitor)
    {
        if (!buffer.TryGetValue(frame, out var frameCmds))
            return;
        for (int pid = 0; pid < frameCmds.Length; pid++)
        {
            var cmd = frameCmds[pid];
            if (cmd == null)
                continue;

            visitor(cmd);
            _commandPool.Release(cmd);
            frameCmds[pid] = null;
        }

        buffer.Remove(frame);
    }

    
}

