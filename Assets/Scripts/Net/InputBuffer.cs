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
        public int seatIndex;
        public int inputType;
        public Vector2 moveVector;
    }
// frame -> [player0Cmd, player1Cmd]
    private readonly Dictionary<int, Command[]> buffer;

    private readonly ObjectPool<Command> _commandPool;
    public  int maxPlayers;
    public InputBuffer() {
        buffer = new Dictionary<int, Command[]>();
        _commandPool = new ObjectPool<Command>(() => new Command());
    }
    public void Push(ResFrameInputData msg) {
        int seatIndex = msg.SeatIndex;
        if ((uint)seatIndex >= (uint)maxPlayers)
            return;

        if (!buffer.TryGetValue(msg.FrameId, out var frameCmds))
        {
            frameCmds = new Command[maxPlayers];
            buffer.Add(msg.FrameId, frameCmds);
        }

        if (frameCmds[seatIndex] != null)
        {
            _commandPool.Release(frameCmds[seatIndex]);
        }

        var cmd = _commandPool.Allocate();
        cmd.frame = msg.FrameId;
        cmd.seatIndex = seatIndex;          
        cmd.inputType = msg.InputType;
        cmd.moveVector = DecodeMoveDir(msg.MoveVector);

        frameCmds[seatIndex] = cmd;
    }
    public void ConsumeFrame(int frame, Action<Command> visitor)
    {
        if (!buffer.TryGetValue(frame, out var frameCmds))
            return;
    
        for (int seatIndex = 0; seatIndex < frameCmds.Length; seatIndex++)
        {
            var cmd = frameCmds[seatIndex];
    
            if (cmd == null)
            {
                // 这里说明你的窗口判断有 bug
                throw new InvalidOperationException(
                    $"Frame {frame} seatIndex {seatIndex} missing input.");
            }
    
            visitor(cmd);
            _commandPool.Release(cmd);
            frameCmds[seatIndex] = null;
        }
    
        buffer.Remove(frame);
    }
    public void SetmaxPlayers(int playerCount) {
        maxPlayers=playerCount;
    }
    public static Vector2 DecodeMoveDir(Vector2D v)
    {
        if (v.X == 0 && v.Y == 0)
            return Vector2.zero;

        return new Vector2(v.X, v.Y).normalized;
    }
    // public void ConsumeFrame(int frame, Action<Command> visitor)
    // {
    //     if (!buffer.TryGetValue(frame, out var frameCmds))
    //     {
    //         // 整帧都没到，直接用预测
    //         SimulateNoneFrame(frame, visitor);
    //         return;
    //     }
    //
    //     for (int seatIndex = 0; seatIndex < frameCmds.Length; seatIndex++)
    //     {
    //         var cmd = frameCmds[seatIndex];
    //
    //         if (cmd == null)
    //         {
    //             // 缺失玩家输入 → 用 NoneInput 或 上一帧输入
    //             cmd = CreatePredictedInput(seatIndex);
    //         }
    //
    //         visitor(cmd);
    //     }
    //
    //     buffer.Remove(frame);
    // }
  
}

